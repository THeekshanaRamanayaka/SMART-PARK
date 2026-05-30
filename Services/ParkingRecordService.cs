using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPark.Data;
using SmartPark.Models;

namespace SmartPark.Services;

public class ParkingRecordService
{
    private readonly SmartParkDbContext _context;
    private readonly ParkingSlotService _slotService;
    private readonly ILogger<ParkingRecordService> _logger;

    public ParkingRecordService(SmartParkDbContext context, ParkingSlotService slotService, ILogger<ParkingRecordService> logger)
    {
        _context = context;
        _slotService = slotService;
        _logger = logger;
    }

    public async Task<List<ParkingRecord>> GetAllRecordsAsync()
    {
        try
        {
            _logger.LogInformation("Loading all parking records");
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .OrderByDescending(r => r.EntryTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load all parking records");
            return new List<ParkingRecord>();
        }
    }

    public async Task<List<ParkingRecord>> GetActiveRecordsAsync()
    {
        try
        {
            _logger.LogInformation("Loading active parking records");
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .Where(r => !r.IsCompleted)
                .OrderByDescending(r => r.EntryTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active parking records");
            return new List<ParkingRecord>();
        }
    }

    public async Task<List<ParkingRecord>> GetCompletedRecordsAsync()
    {
        try
        {
            _logger.LogInformation("Loading completed parking records");
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .Where(r => r.IsCompleted)
                .OrderByDescending(r => r.ExitTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load completed parking records");
            return new List<ParkingRecord>();
        }
    }

    public async Task<ParkingRecord?> GetRecordByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading parking record {RecordId}", id);
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load parking record {RecordId}", id);
            return null;
        }
    }

    public async Task<List<ParkingRecord>> SearchRecordsAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching parking records for {SearchTerm}", searchTerm);
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .Where(r => r.VehicleNumber.Contains(searchTerm) ||
                           r.ParkingSlot!.SlotNumber.Contains(searchTerm))
                .OrderByDescending(r => r.EntryTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search parking records for {SearchTerm}", searchTerm);
            return new List<ParkingRecord>();
        }
    }

    /// <summary>
    /// Server-side paginated view over parking records, optionally filtered by
    /// vehicle/slot number. Pagination and counting happen in SQL via
    /// <c>Skip</c>/<c>Take</c>/<c>CountAsync</c>, so the application never
    /// materialises the full history table — keeping memory and network cost
    /// constant as the records table grows.
    /// </summary>
    public async Task<PagedResult<ParkingRecord>> GetRecordsPageAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? completed = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var query = BuildRecordsQuery(searchTerm, completed);

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(r => r.ParkingSlot)
                .OrderByDescending(r => r.EntryTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Loaded parking records page {Page} (size {PageSize}, total {Total}, search='{Search}', completed={Completed})",
                page, pageSize, totalCount, searchTerm ?? string.Empty, completed);

            return new PagedResult<ParkingRecord>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load parking records page {Page}", page);
            return PagedResult<ParkingRecord>.Empty(page, pageSize);
        }
    }

    /// <summary>
    /// Server-side paginated view restricted to completed sessions. Used by the
    /// Reports page; aggregate stats (total/average duration, completed count)
    /// are computed separately in <see cref="GetCompletedSummaryAsync"/> so they
    /// stay accurate regardless of the currently visible page.
    /// </summary>
    public Task<PagedResult<ParkingRecord>> GetCompletedRecordsPageAsync(int page, int pageSize)
        => GetRecordsPageAsync(page, pageSize, searchTerm: null, completed: true);

    /// <summary>
    /// Aggregate metrics for the Reports page. Computed in SQL so it does not
    /// depend on which page of detail rows the UI happens to be displaying.
    /// </summary>
    public async Task<CompletedRecordsSummary> GetCompletedSummaryAsync()
    {
        try
        {
            // Reuse the completed-record projection that already powers the
            // report table. This keeps the summary aligned with the rows shown
            // on screen and avoids provider-specific issues around aggregating
            // MySQL TIME values in SQL.
            var completedRecords = await GetCompletedRecordsAsync();
            var durations = completedRecords
                .Where(r => r.Duration.HasValue)
                .Select(r => r.Duration!.Value)
                .ToList();

            var totalCount = completedRecords.Count;

            var total = durations.Count == 0
                ? TimeSpan.Zero
                : durations.Aggregate(TimeSpan.Zero, (acc, d) => acc + d);

            var average = durations.Count == 0
                ? TimeSpan.Zero
                : TimeSpan.FromTicks(total.Ticks / durations.Count);

            return new CompletedRecordsSummary
            {
                CompletedCount = totalCount,
                TotalDuration = total,
                AverageDuration = average
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load completed-records summary");
            return new CompletedRecordsSummary();
        }
    }

    private IQueryable<ParkingRecord> BuildRecordsQuery(string? searchTerm, bool? completed)
    {
        IQueryable<ParkingRecord> query = _context.ParkingRecords.AsNoTracking();

        if (completed.HasValue)
        {
            query = query.Where(r => r.IsCompleted == completed.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(r =>
                r.VehicleNumber.Contains(term) ||
                r.ParkingSlot!.SlotNumber.Contains(term));
        }

        return query;
    }

    public async Task<List<ParkingRecord>> GetVehicleHistoryAsync(string vehicleNumber)
    {
        var normalizedVehicleNumber = vehicleNumber.Trim().ToUpper();

        try
        {
            _logger.LogInformation("Loading vehicle history for {VehicleNumber}", normalizedVehicleNumber);
            return await _context.ParkingRecords
                .Include(r => r.ParkingSlot)
                .Where(r => r.IsCompleted && r.VehicleNumber.ToUpper() == normalizedVehicleNumber)
                .OrderByDescending(r => r.EntryTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load vehicle history for {VehicleNumber}", normalizedVehicleNumber);
            return new List<ParkingRecord>();
        }
    }

    public async Task<bool> RecordVehicleEntryAsync(string vehicleNumber, string ownerName, int? slotId = null)
    {
        // Use an explicit transaction to keep vehicle creation, record creation
        // and slot update atomic.
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Recording vehicle entry for {VehicleNumber} (slotId={SlotId})", vehicleNumber, slotId);

            var plateNorm = vehicleNumber.Trim().ToUpper();
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.PlateNormalized == plateNorm);
            if (vehicle == null)
            {
                vehicle = new Vehicle
                {
                    Plate = vehicleNumber.Trim(),
                    PlateNormalized = plateNorm,
                    OwnerName = ownerName,
                    LastSeen = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
            }
            else
            {
                vehicle.OwnerName = ownerName;
                vehicle.LastSeen = DateTime.UtcNow;
                vehicle.UpdatedAt = DateTime.UtcNow;
                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
            }

            ParkingSlot? targetSlot;
            if (slotId.HasValue)
            {
                targetSlot = await _context.ParkingSlots.FindAsync(slotId.Value);
                if (targetSlot == null || targetSlot.IsOccupied)
                {
                    _logger.LogWarning("Requested slot {SlotId} is unavailable for vehicle {VehicleNumber}", slotId, vehicleNumber);
                    await tx.RollbackAsync();
                    return false;
                }
            }
            else
            {
                targetSlot = await _slotService.GetAvailableSlotAsync();
                if (targetSlot == null)
                {
                    _logger.LogWarning("No available slot found for vehicle {VehicleNumber}", vehicleNumber);
                    await tx.RollbackAsync();
                    return false;
                }
            }

            var record = new ParkingRecord
            {
                VehicleNumber = vehicleNumber,
                OwnerName = ownerName,
                VehicleId = vehicle.Id,
                ParkingSlotId = targetSlot.Id,
                EntryTime = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.ParkingRecords.Add(record);
            // Update slot occupied status
            var updated = await _slotService.UpdateSlotStatusAsync(targetSlot.Id, true);
            if (!updated)
            {
                _logger.LogWarning("Failed to update slot status for slot {SlotId}", targetSlot.Id);
                await tx.RollbackAsync();
                return false;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            _logger.LogInformation("Vehicle {VehicleNumber} assigned to slot {SlotNumber}", vehicleNumber, targetSlot.SlotNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record vehicle entry for {VehicleNumber}", vehicleNumber);
            try { await tx.RollbackAsync(); } catch { }
            return false;
        }
    }

    /// <summary>
    /// Returns a distinct list of previously-seen vehicles together with the most recent owner name,
    /// used to power the searchable vehicle dropdown on the entry screen.
    /// </summary>
    public async Task<List<VehicleSummary>> GetKnownVehiclesAsync()
    {
        try
        {
            _logger.LogInformation("Loading known vehicles for autocomplete");

            // Prefer the normalized Vehicles table when present (fast indexed lookup).
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.Plate)
                .ToListAsync();

            if (vehicles != null && vehicles.Count > 0)
            {
                return vehicles.Select(v => new VehicleSummary
                {
                    VehicleNumber = v.Plate,
                    OwnerName = v.OwnerName ?? string.Empty,
                    LastSeen = v.LastSeen ?? v.CreatedAt
                }).OrderBy(v => v.VehicleNumber).ToList();
            }

            // Fallback: infer from ParkingRecords if Vehicles is empty (back-compat)
            var records = await _context.ParkingRecords
                .AsNoTracking()
                .OrderByDescending(r => r.EntryTime)
                .Select(r => new { r.VehicleNumber, r.OwnerName, r.EntryTime })
                .ToListAsync();

            return records
                .GroupBy(r => r.VehicleNumber.Trim().ToUpper())
                .Select(g =>
                {
                    var latest = g.First();
                    return new VehicleSummary
                    {
                        VehicleNumber = latest.VehicleNumber,
                        OwnerName = latest.OwnerName,
                        LastSeen = latest.EntryTime
                    };
                })
                .OrderBy(v => v.VehicleNumber)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load known vehicles");
            return new List<VehicleSummary>();
        }
    }

    /// <summary>
    /// Server-side paginated view over *active* (incomplete) parking records.
    /// Used by Dashboard active tab and Active Parking page for efficient browsing
    /// without materializing the entire active session list.
    /// </summary>
    public async Task<PagedResult<ParkingRecord>> GetActiveRecordsPageAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var query = _context.ParkingRecords
                .AsNoTracking()
                .Where(r => !r.IsCompleted);

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(r => r.ParkingSlot)
                .OrderByDescending(r => r.EntryTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("Loaded active parking records page {Page} (size {PageSize}, total {Total})",
                page, pageSize, totalCount);

            return new PagedResult<ParkingRecord>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active parking records page {Page}", page);
            return PagedResult<ParkingRecord>.Empty(page, pageSize);
        }
    }

    /// <summary>
    /// Server-side paginated view over all records grouped and aggregated by vehicle number.
    /// Includes session counts, duration metrics, and last-seen timestamp for each vehicle.
    /// </summary>
    public async Task<PagedResult<VehicleGroupedRecords>> GetRecordsGroupedByVehiclePageAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            // Project raw records into a form we can group client-side (small overhead
            // for grouping logic compared to doing aggregation in SQL).
            var allRecords = await _context.ParkingRecords
                .AsNoTracking()
                .Select(r => new
                {
                    r.VehicleNumber,
                    r.OwnerName,
                    r.IsCompleted,
                    r.Duration,
                    r.EntryTime
                })
                .ToListAsync();

            // Group by vehicle, compute aggregates.
            var grouped = allRecords
                .GroupBy(r => r.VehicleNumber.Trim().ToUpper())
                .Select(g =>
                {
                    var first = g.First(); // Latest entry to get owner name
                    var latest = g.OrderByDescending(x => x.EntryTime).First();
                    var completed = g.Where(x => x.IsCompleted).ToList();
                    var durations = completed
                        .Where(x => x.Duration.HasValue)
                        .Select(x => x.Duration!.Value)
                        .ToList();

                    var totalDuration = durations.Any()
                        ? durations.Aggregate(TimeSpan.Zero, (acc, d) => acc + d)
                        : TimeSpan.Zero;

                    var avgDuration = durations.Any()
                        ? TimeSpan.FromTicks(durations.Sum(d => d.Ticks) / durations.Count)
                        : TimeSpan.Zero;

                    return new VehicleGroupedRecords
                    {
                        VehicleNumber = first.VehicleNumber,
                        OwnerName = first.OwnerName,
                        TotalSessions = g.Count(),
                        CompletedSessions = completed.Count,
                        ActiveSessions = g.Count(x => !x.IsCompleted),
                        TotalDuration = totalDuration,
                        AverageDuration = avgDuration,
                        LastSeen = latest.EntryTime
                    };
                })
                .OrderByDescending(v => v.LastSeen)
                .ToList();

            var totalCount = grouped.Count;
            var items = grouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _logger.LogInformation("Loaded grouped records (vehicles) page {Page} (size {PageSize}, total {Total})",
                page, pageSize, totalCount);

            return new PagedResult<VehicleGroupedRecords>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load grouped records page {Page}", page);
            return PagedResult<VehicleGroupedRecords>.Empty(page, pageSize);
        }
    }

    /// <summary>
    /// Server-side paginated view over completed records only, grouped and aggregated
    /// by vehicle number. Excludes active sessions.
    /// </summary>
    public async Task<PagedResult<VehicleGroupedRecords>> GetCompletedRecordsGroupedByVehiclePageAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            var completedRecords = await _context.ParkingRecords
                .AsNoTracking()
                .Where(r => r.IsCompleted)
                .Select(r => new
                {
                    r.VehicleNumber,
                    r.OwnerName,
                    r.Duration,
                    r.EntryTime
                })
                .ToListAsync();

            var grouped = completedRecords
                .GroupBy(r => r.VehicleNumber.Trim().ToUpper())
                .Select(g =>
                {
                    var first = g.First();
                    var latest = g.OrderByDescending(x => x.EntryTime).First();
                    var durations = g
                        .Where(x => x.Duration.HasValue)
                        .Select(x => x.Duration!.Value)
                        .ToList();

                    var totalDuration = durations.Any()
                        ? durations.Aggregate(TimeSpan.Zero, (acc, d) => acc + d)
                        : TimeSpan.Zero;

                    var avgDuration = durations.Any()
                        ? TimeSpan.FromTicks(durations.Sum(d => d.Ticks) / durations.Count)
                        : TimeSpan.Zero;

                    return new VehicleGroupedRecords
                    {
                        VehicleNumber = first.VehicleNumber,
                        OwnerName = first.OwnerName,
                        TotalSessions = 0, // Excluded for "completed only" view
                        CompletedSessions = g.Count(),
                        ActiveSessions = 0,
                        TotalDuration = totalDuration,
                        AverageDuration = avgDuration,
                        LastSeen = latest.EntryTime
                    };
                })
                .OrderByDescending(v => v.LastSeen)
                .ToList();

            var totalCount = grouped.Count;
            var items = grouped
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<VehicleGroupedRecords>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load grouped completed records page {Page}", page);
            return PagedResult<VehicleGroupedRecords>.Empty(page, pageSize);
        }
    }

    public async Task<bool> RecordVehicleExitAsync(int recordId)
    {
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            _logger.LogInformation("Recording vehicle exit for record {RecordId}", recordId);
            var record = await _context.ParkingRecords.FindAsync(recordId);
            if (record == null)
            {
                _logger.LogWarning("Record {RecordId} was not found", recordId);
                await tx.RollbackAsync();
                return false;
            }

            if (record.IsCompleted)
            {
                _logger.LogWarning("Record {RecordId} was already completed", recordId);
                await tx.RollbackAsync();
                return false;
            }

            record.ExitTime = DateTime.UtcNow;
            record.Duration = record.ExitTime - record.EntryTime;
            record.IsCompleted = true;

            var updated = await _slotService.UpdateSlotStatusAsync(record.ParkingSlotId, false);
            if (!updated)
            {
                _logger.LogWarning("Failed to update slot status for slot {SlotId} during exit", record.ParkingSlotId);
                await tx.RollbackAsync();
                return false;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            _logger.LogInformation("Vehicle exit recorded for record {RecordId}", recordId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record vehicle exit for record {RecordId}", recordId);
            try { await tx.RollbackAsync(); } catch { }
            return false;
        }
    }
}
