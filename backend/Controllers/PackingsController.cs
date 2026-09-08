using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackendProject.Data;
using BackendProject.Models;

namespace BackendProject.Controllers
{
    [Authorize]          // a valid token is required
    [ApiController]
    [Route("api/[controller]")]
    public class PackingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PackingsController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetPackings(
            string? search = null,
            string? id = null,
            string? size = null,
            string? packingName = null,
            string? sortBy = null,
            string? order = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Packings.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.PackingName.Contains(search));

            if (decimal.TryParse(size, out var sizeValue))
                query = query.Where(p => p.Size == sizeValue);

            if (!string.IsNullOrWhiteSpace(packingName))
                query = query.Where(p => p.PackingName.Contains(packingName));

            if (int.TryParse(id, out var idValue))
                query = query.Where(p => p.Id == idValue);

            var total = await query.CountAsync();

            bool desc = order?.ToLower() == "desc";

            query = sortBy?.ToLower() switch
            {
                "size"   => desc ? query.OrderByDescending(p => p.Size)   : query.OrderBy(p => p.Size),
                "packingname" => desc ? query.OrderByDescending(p => p.PackingName) : query.OrderBy(p => p.PackingName),
                "id"     => desc ? query.OrderByDescending(p => p.Id)     : query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id),
            };

            pageSize = Math.Clamp(pageSize <= 0 ? 200 : pageSize, 1, 200);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var items = await query
                .Select(p => new { p.Id, p.PackingName, p.Size })
                .ToListAsync();

            return Ok(new { items, total });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPacking(int id)
        {
            var packing = await _context.Packings.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new { p.Id, p.PackingName, p.Size })
                .FirstOrDefaultAsync();

            if (packing == null) return NotFound();
            return Ok(packing);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePacking(PackingDto dto)
        {
            var packing = new Packing { PackingName = dto.PackingName.Trim(), Size = dto.Size };

            _context.Packings.Add(packing);
            await _context.SaveChangesAsync();

            return Ok(new { packing.Id, packing.PackingName, packing.Size });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePacking(int id, PackingDto dto)
        {
            var packing = await _context.Packings.FindAsync(id);
            if (packing == null) return NotFound();

            packing.PackingName = dto.PackingName.Trim();
            packing.Size = dto.Size;

            await _context.SaveChangesAsync();

            return Ok(new { packing.Id, packing.PackingName, packing.Size });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePacking(int id)
        {
            var packing = await _context.Packings.FindAsync(id);
            if (packing == null) return NotFound();

            _context.Packings.Remove(packing);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
