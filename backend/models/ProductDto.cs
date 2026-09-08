using System.ComponentModel.DataAnnotations;

namespace BackendProject.Models;

public class ProductDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
    public decimal Discount { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "Stock cannot be negative.")]
    public int Stock { get; set; }

    // the shades and packings ticked in the dialog -> bridge rows
    public List<int> ShadeIds { get; set; } = new();
    public List<int> PackingIds { get; set; } = new();
}

public class ShadeDto
{
    [Required(ErrorMessage = "Shade name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Shade name must be between 2 and 100 characters.")]
    public string ShadeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pick a colour.")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Colour must be a hex code like #1A2B3C.")]
    public string ColorCode { get; set; } = string.Empty;
}

public class PackingDto
{
    [Required(ErrorMessage = "Packing name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Packing name must be between 2 and 100 characters.")]
    public string PackingName { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage = "Size must be greater than 0.")]
    public decimal Size { get; set; }
}
