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
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/Products
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            string? search = null,
            string? id = null,
            string? title = null,
            string? category = null,
            string? discount = null,
            string? price = null,
            string? stock = null,
            string? sortBy = null,
            string? order = null,
            int page = 1,
            int pageSize = 10)
        {
            // AsNoTracking: we are only READING. Skipping change
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search)
                                      || p.Category.Contains(search));

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(p => p.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category.Contains(category));

            if (int.TryParse(id, out var idValue))
                query = query.Where(p => p.Id == idValue);

            if (decimal.TryParse(discount, out var discountValue))
                query = query.Where(p => p.Discount == discountValue);

            if (decimal.TryParse(price, out var priceValue))
                query = query.Where(p => p.Price == priceValue);

            if (int.TryParse(stock, out var stockValue))
                query = query.Where(p => p.Stock == stockValue);

            // after filtering, before paging
            var total = await query.CountAsync();

            bool desc = order?.ToLower() == "desc";

            query = sortBy?.ToLower() switch
            {
                "title"    => desc ? query.OrderByDescending(p => p.Title)    : query.OrderBy(p => p.Title),
                "category" => desc ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
                "discount" => desc ? query.OrderByDescending(p => p.Discount) : query.OrderBy(p => p.Discount),
                "price"    => desc ? query.OrderByDescending(p => p.Price)    : query.OrderBy(p => p.Price),
                "stock"    => desc ? query.OrderByDescending(p => p.Stock)    : query.OrderBy(p => p.Stock),
                "id"       => desc ? query.OrderByDescending(p => p.Id)       : query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id),   // newest first
            };

            pageSize = Math.Clamp(pageSize <= 0 ? 200 : pageSize, 1, 200);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            var items = await query
                .Select(p => new
                {
                    p.Id, p.Title, p.Category, p.Discount, p.Price, p.Stock
                })
                .ToListAsync();

            return Ok(new { items, total });
        }

        // GET /api/Products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id, p.Title, p.Category, p.Discount, p.Price, p.Stock,
                    ShadeIds   = p.ProductShades.Select(ps => ps.ShadeId).ToList(),
                    PackingIds = p.ProductPackings.Select(pp => pp.PackingId).ToList(),
                })
                .FirstOrDefaultAsync();

            if (product == null) return NotFound();

            return Ok(product);
        }

        // POST /api/Products
        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto dto)
        {
            var product = new Product
            {
                Title    = dto.Title,
                Category = dto.Category,
                Discount = dto.Discount,
                Price    = dto.Price,
                Stock    = dto.Stock,
            };

            foreach (var shadeId in dto.ShadeIds.Distinct())
                product.ProductShades.Add(new ProductShade { ShadeId = shadeId });

            foreach (var packingId in dto.PackingIds.Distinct())
                product.ProductPackings.Add(new ProductPacking { PackingId = packingId });

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { product.Id, product.Title, product.Category,
                            product.Discount, product.Price, product.Stock });
        }

        // PUT /api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.ProductShades)
                .Include(p => p.ProductPackings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            product.Title    = dto.Title;
            product.Category = dto.Category;
            product.Discount = dto.Discount;
            product.Price    = dto.Price;
            product.Stock    = dto.Stock;

            // throw the old links away, add the ticked ones
            product.ProductShades.Clear();
            foreach (var shadeId in dto.ShadeIds.Distinct())
                product.ProductShades.Add(new ProductShade { ShadeId = shadeId });

            product.ProductPackings.Clear();
            foreach (var packingId in dto.PackingIds.Distinct())
                product.ProductPackings.Add(new ProductPacking { PackingId = packingId });

            await _context.SaveChangesAsync();

            return Ok(new { product.Id, product.Title, product.Category,
                            product.Discount, product.Price, product.Stock });
        }

        // DELETE /api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
