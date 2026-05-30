namespace SmartPark.Models;

public class ParkingRecord
{
    public int Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public int ParkingSlotId { get; set; }
    public ParkingSlot? ParkingSlot { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool IsCompleted { get; set; }
}
