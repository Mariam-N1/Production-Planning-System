namespace BackendProject.Models;

// Bridge table: one row = "this product comes in this shade".
public class ProductShade
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int ShadeId { get; set; }
    public Shade? Shade { get; set; }
}
