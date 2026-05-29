namespace SmartPark.Models;

/// <summary>
/// Lightweight projection used by the entry-screen vehicle autocomplete.
/// Represents a previously-seen vehicle and its most recent owner name.
/// </summary>
public class VehicleSummary
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
}
