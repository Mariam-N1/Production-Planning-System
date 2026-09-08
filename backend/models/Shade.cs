namespace BackendProject.Models;

// The Shades screen.
public class Shade
{
    public int Id { get; set; }
    public string ShadeName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;   // e.g. #0b6e4f - unique

    public List<ProductShade> ProductShades { get; set; } = new();
}
