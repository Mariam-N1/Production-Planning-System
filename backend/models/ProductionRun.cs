namespace BackendProject.Models;

// One request to make a batch of paint.
// Pending -> Approved -> Completed, or Pending -> Rejected.
public class ProductionRun
{
    public int Id { get; set; }

    // what to make - only the ids are stored, never the names
    public int ProductId { get; set; }
    public int ShadeId { get; set; }
    public int PackingId { get; set; }

    public int Quantity { get; set; }              // how many tins / buckets / drums
    public string Note { get; set; } = string.Empty;

    public string Status { get; set; } = RunStatus.Pending;
    public string Decision { get; set; } = string.Empty;   // why it was rejected

    public string RequestedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecidedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // EF follows these to reach the real rows
    public Product? Product { get; set; }
    public Shade? Shade { get; set; }
    public Packing? Packing { get; set; }
}

public static class RunStatus
{
    public const string Pending   = "Pending";
    public const string Approved  = "Approved";
    public const string Completed = "Completed";
    public const string Rejected  = "Rejected";

    public static readonly string[] All = { Pending, Approved, Completed, Rejected };
}
