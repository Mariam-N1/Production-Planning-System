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
    public class ShadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShadesController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetShades(
            string? search = null,
            string? id = null,
            string? shadeName = null,
            string? colorCode = null,
            string? sortBy = null,
            string? order = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Shades.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.ShadeName.Contains(search));

            if (!string.IsNullOrWhiteSpace(shadeName))
                query = query.Where(s => s.ShadeName.Contains(shadeName));

            if (!string.IsNullOrWhiteSpace(colorCode))
                query = query.Where(s => s.ColorCode.Contains(colorCode));

            if (int.TryParse(id, out var idValue))
                query = query.Where(s => s.Id == idValue);

            // after filtering, before paging
            var total = await query.CountAsync();

            bool desc = order?.ToLower() == "desc";

            query = sortBy?.ToLower() switch
            {
                "shadename" => desc ? query.OrderByDescending(s => s.ShadeName) : query.OrderBy(s => s.ShadeName),
                "colorcode" => desc ? query.OrderByDescending(s => s.ColorCode) : query.OrderBy(s => s.ColorCode),
                "id"        => desc ? query.OrderByDescending(s => s.Id)        : query.OrderBy(s => s.Id),
                _ => query.OrderByDescending(s => s.Id),
            };

            pageSize = Math.Clamp(pageSize <= 0 ? 200 : pageSize, 1, 200);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var items = await query
                .Select(s => new { s.Id, s.ShadeName, s.ColorCode })
                .ToListAsync();

            return Ok(new { items, total });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShade(int id)
        {
            var shade = await _context.Shades.AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new { s.Id, s.ShadeName, s.ColorCode })
                .FirstOrDefaultAsync();

            if (shade == null) return NotFound();
            return Ok(shade);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShade(ShadeDto dto)
        {
            var taken = await HexTakenAsync(dto.ColorCode, null);
            if (taken != null) return BadRequest(taken);

            var shade = new Shade
            {
                ShadeName = dto.ShadeName.Trim(),
                ColorCode = dto.ColorCode.Trim().ToLower(),
            };

            _context.Shades.Add(shade);
            await _context.SaveChangesAsync();

            return Ok(new { shade.Id, shade.ShadeName, shade.ColorCode });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShade(int id, ShadeDto dto)
        {
            var shade = await _context.Shades.FindAsync(id);
            if (shade == null) return NotFound();

            var taken = await HexTakenAsync(dto.ColorCode, id);
            if (taken != null) return BadRequest(taken);

            shade.ShadeName = dto.ShadeName.Trim();
            shade.ColorCode = dto.ColorCode.Trim().ToLower();

            await _context.SaveChangesAsync();

            return Ok(new { shade.Id, shade.ShadeName, shade.ColorCode });
        }

        // A hex belongs to one shade only. The database has a unique
        // index too - this just turns it into a readable message.
        private async Task<object?> HexTakenAsync(string colorCode, int? ignoreId)
        {
            var hex = colorCode.Trim().ToLower();

            var owner = await _context.Shades
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ColorCode == hex && (ignoreId == null || s.Id != ignoreId));

            if (owner == null) return null;

            return new
            {
                errors = new Dictionary<string, string[]>
                {
                    ["ColorCode"] = new[] { $"{hex} is already used by \"{owner.ShadeName}\". Pick a different colour." }
                }
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShade(int id)
        {
            var shade = await _context.Shades.FindAsync(id);
            if (shade == null) return NotFound();

            _context.Shades.Remove(shade);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
