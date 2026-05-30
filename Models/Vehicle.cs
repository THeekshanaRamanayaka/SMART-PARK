namespace SmartPark.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Plate { get; set; } = string.Empty;
    public string PlateNormalized { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public int? AppUserId { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public byte[]? RowVersion { get; set; }
}
