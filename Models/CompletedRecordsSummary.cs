namespace SmartPark.Models;

/// <summary>
/// Aggregate metrics for the Reports page. Calculated server-side so the
/// numbers remain accurate even when the UI only loads one page of detail
/// rows at a time.
/// </summary>
public class CompletedRecordsSummary
{
    public int CompletedCount { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public TimeSpan AverageDuration { get; init; }
}
