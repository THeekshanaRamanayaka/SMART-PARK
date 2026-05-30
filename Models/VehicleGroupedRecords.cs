namespace SmartPark.Models;

/// <summary>
/// Groups all parking records for a single vehicle with aggregated metrics.
/// Used for vehicle-centered reporting and drill-down analysis.
/// </summary>
public class VehicleGroupedRecords
{
    public string VehicleNumber { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    
    /// <summary>
    /// Total sessions (entries) for this vehicle across all time.
    /// </summary>
    public int TotalSessions { get; init; }
    
    /// <summary>
    /// Completed sessions only.
    /// </summary>
    public int CompletedSessions { get; init; }
    
    /// <summary>
    /// Active (in-progress) sessions for this vehicle.
    /// </summary>
    public int ActiveSessions { get; init; }
    
    /// <summary>
    /// Sum of durations for all completed sessions.
    /// </summary>
    public TimeSpan TotalDuration { get; init; }
    
    /// <summary>
    /// Average duration across completed sessions.
    /// </summary>
    public TimeSpan AverageDuration { get; init; }
    
    /// <summary>
    /// Last entry time for this vehicle.
    /// </summary>
    public DateTime LastSeen { get; init; }
}
