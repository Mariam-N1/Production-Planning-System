using System.ComponentModel.DataAnnotations;

namespace BackendProject.Models;

public class ProductionRunDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a product.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a shade.")]
    public int ShadeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a pack size.")]
    public int PackingId { get; set; }

    [Range(1, 1000000, ErrorMessage = "Units must be between 1 and 1,000,000.")]
    public int Quantity { get; set; }

    [StringLength(300, ErrorMessage = "The note cannot be longer than 300 characters.")]
    public string Note { get; set; } = string.Empty;
}

public class RejectDto
{
    [StringLength(300, ErrorMessage = "The reason cannot be longer than 300 characters.")]
    public string Reason { get; set; } = string.Empty;
}
