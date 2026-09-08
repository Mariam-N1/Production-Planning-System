namespace BackendProject.Models;

// Bridge table: one row = "this product is sold in this packing".
public class ProductPacking
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int PackingId { get; set; }
    public Packing? Packing { get; set; }
}
