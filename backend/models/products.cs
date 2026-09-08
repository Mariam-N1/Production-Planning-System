namespace BackendProject.Models;

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    // not columns - EF uses these to reach the linked rows
    public List<ProductShade> ProductShades { get; set; } = new();
    public List<ProductPacking> ProductPackings { get; set; } = new();
}
