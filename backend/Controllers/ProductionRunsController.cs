using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BackendProject.Data;
using BackendProject.Models;

namespace BackendProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionRunsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductionRunsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/ProductionRuns?status=Pending
        [HttpGet]
        public async Task<IActionResult> GetAll(string? status)
        {
            var query = _context.ProductionRuns.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!RunStatus.All.Contains(status))
                    return BadRequest(Error("Status", "Unknown status."));

                query = query.Where(r => r.Status == status);
            }

            // newest first, and the names come from the linked rows
            var items = await query
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    r.Id,
                    r.Quantity,
                    r.Note,
                    r.Status,
                    r.Decision,
                    r.RequestedBy,
                    r.CreatedAt,
                    r.DecidedAt,
                    r.CompletedAt,

                    r.ProductId,
                    ProductTitle = r.Product!.Title,

                    r.ShadeId,
                    ShadeName = r.Shade!.ShadeName,
                    r.Shade!.ColorCode,

                    r.PackingId,
                    PackingName = r.Packing!.PackingName,
                    PackingSize = r.Packing!.Size,

                    Litres = r.Quantity * r.Packing!.Size,
                })
                .ToListAsync();

            // one call feeds the tab counts too, so the screen needs no extra requests
            var counts = await _context.ProductionRuns.AsNoTracking()
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int countOf(string s) => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

            var pendingLitres = await _context.ProductionRuns.AsNoTracking()
                .Where(r => r.Status == RunStatus.Pending)
                .SumAsync(r => (decimal?)(r.Quantity * r.Packing!.Size)) ?? 0m;

            var oldestPending = await _context.ProductionRuns.AsNoTracking()
                .Where(r => r.Status == RunStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .Select(r => (DateTime?)r.CreatedAt)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                items,
                counts = new
                {
                    pending   = countOf(RunStatus.Pending),
                    approved  = countOf(RunStatus.Approved),
                    completed = countOf(RunStatus.Completed),
                    rejected  = countOf(RunStatus.Rejected),
                },
                pendingLitres,
                oldestPending,
            });
        }

        // POST /api/ProductionRuns
        [HttpPost]
        public async Task<IActionResult> Create(ProductionRunDto dto)
        {
            // the three rows must actually exist before we point at them
            if (!await _context.Products.AnyAsync(p => p.Id == dto.ProductId))
                return BadRequest(Error("ProductId", "That product no longer exists."));

            if (!await _context.Shades.AnyAsync(s => s.Id == dto.ShadeId))
                return BadRequest(Error("ShadeId", "That shade no longer exists."));

            if (!await _context.Packings.AnyAsync(p => p.Id == dto.PackingId))
                return BadRequest(Error("PackingId", "That pack size no longer exists."));

            var run = new ProductionRun
            {
                ProductId = dto.ProductId,
                ShadeId = dto.ShadeId,
                PackingId = dto.PackingId,
                Quantity = dto.Quantity,
                Note = (dto.Note ?? string.Empty).Trim(),
                Status = RunStatus.Pending,
                RequestedBy = WhoAmI(),
                CreatedAt = DateTime.UtcNow,
            };

            _context.ProductionRuns.Add(run);
            await _context.SaveChangesAsync();

            return Ok(new { run.Id });
        }

        // PUT /api/ProductionRuns/5/approve  - authorises it, nothing else changes
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var run = await _context.ProductionRuns.FindAsync(id);
            if (run == null) return NotFound();

            if (run.Status != RunStatus.Pending)
                return BadRequest(Error("Status", "Only a pending request can be approved."));

            run.Status = RunStatus.Approved;
            run.DecidedAt = DateTime.UtcNow;
            run.Decision = string.Empty;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT /api/ProductionRuns/5/reject
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id, RejectDto dto)
        {
            var run = await _context.ProductionRuns.FindAsync(id);
            if (run == null) return NotFound();

            if (run.Status != RunStatus.Pending)
                return BadRequest(Error("Status", "Only a pending request can be rejected."));

            run.Status = RunStatus.Rejected;
            run.DecidedAt = DateTime.UtcNow;
            run.Decision = (dto.Reason ?? string.Empty).Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT /api/ProductionRuns/5/complete  - the paint got made, so stock goes up
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var run = await _context.ProductionRuns.FindAsync(id);
            if (run == null) return NotFound();

            if (run.Status != RunStatus.Approved)
                return BadRequest(Error("Status", "Only an approved run can be marked as made."));

            var product = await _context.Products.FindAsync(run.ProductId);
            if (product == null)
                return BadRequest(Error("ProductId", "That product no longer exists."));

            // the one place this screen writes to your Products table
            product.Stock += run.Quantity;

            run.Status = RunStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { product.Id, product.Stock });
        }

        // the signed-in person's name, straight out of the token
        private string WhoAmI()
        {
            return this.User.FindFirstValue("name")
                ?? this.User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? "Unknown";
        }

        private static object Error(string field, string message)
            => new { errors = new Dictionary<string, string[]> { [field] = new[] { message } } };
    }
}
