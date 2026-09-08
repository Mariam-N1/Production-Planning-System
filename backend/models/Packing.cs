namespace BackendProject.Models;

// The Packing screen.
public class Packing
{
    public int Id { get; set; }
    public string PackingName { get; set; } = string.Empty;   // e.g. Bucket
    public decimal Size { get; set; }                          // litres

    public List<ProductPacking> ProductPackings { get; set; } = new();
}
