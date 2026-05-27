namespace SmartPark.Models;

public class ParkingSlot
{
    public int Id { get; set; }
    public string SlotNumber { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public DateTime? LastUpdated { get; set; }
}
