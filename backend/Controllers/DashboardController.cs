using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackendProject.Data;

namespace BackendProject.Controllers
{
    // Everything the Home screen needs, in one request.
    [Authorize]          // a valid token is required
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            // ---------- counts ----------
            var productCount = await _context.Products.CountAsync();
            var shadeCount   = await _context.Shades.CountAsync();
            var packingCount = await _context.Packings.CountAsync();

            // SumAsync on an empty table throws, so guard with a nullable sum
            var stockValue = await _context.Products
                .SumAsync(p => (decimal?)(p.Price * p.Stock)) ?? 0m;

            // ---------- sellable combinations ----------
            // shades x packings, per product, added up
            var skuCount = await _context.Products
                .SumAsync(p => (int?)(p.ProductShades.Count * p.ProductPackings.Count)) ?? 0;

            // ---------- the fan deck ----------
            var shades = await _context.Shades
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .Take(10)
                .Select(s => new { s.Id, s.ShadeName, s.ColorCode })
                .ToListAsync();

            // ---------- recent products, with their shade colours ----------
            var recent = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Take(7)
                .Select(p => new
                {
                    p.Id, p.Title, p.Category, p.Price, p.Stock,
                    Hexes = p.ProductShades
                        .OrderBy(ps => ps.Id)
                        .Select(ps => ps.Shade!.ColorCode)
                        .Take(4)
                        .ToList(),
                })
                .ToListAsync();

            // ---------- needs attention ----------
            var noShade   = await _context.Products.CountAsync(p => !p.ProductShades.Any());
            var noPacking = await _context.Products.CountAsync(p => !p.ProductPackings.Any());

            var outOfStock = await _context.Products
                .Where(p => p.Stock == 0)
                .Select(p => p.Title)
                .ToListAsync();

            // shades nobody links to
            var unusedShades = await _context.Shades
                .Where(s => !s.ProductShades.Any())
                .Select(s => s.ShadeName)
                .ToListAsync();

            // ---------- most-used shades ----------
            var topShades = await _context.ProductShades
                .AsNoTracking()
                .GroupBy(ps => new { ps.ShadeId, ps.Shade!.ShadeName, ps.Shade!.ColorCode })
                .Select(g => new
                {
                    g.Key.ShadeId,
                    g.Key.ShadeName,
                    g.Key.ColorCode,
                    Count = g.Count(),
                })
                .OrderByDescending(x => x.Count)
                .Take(3)
                .ToListAsync();

            return Ok(new
            {
                counts = new { products = productCount, shades = shadeCount, packings = packingCount },
                stockValue,
                skuCount,
                shades,
                recent,
                attention = new
                {
                    noShade,
                    noPacking,
                    outOfStock      = outOfStock.Count,
                    outOfStockNames = string.Join(", ", outOfStock.Take(2)),
                    unusedShades      = unusedShades.Count,
                    unusedShadeNames  = string.Join(", ", unusedShades.Take(2)),
                },
                topShades,
            });
        }
    }
}
