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
        try
        {
            _logger.LogInformation("Recording vehicle entry for {VehicleNumber} (slotId={SlotId})", vehicleNumber, slotId);

            ParkingSlot? targetSlot;
            if (slotId.HasValue)
            {
                targetSlot = await _context.ParkingSlots.FindAsync(slotId.Value);
                if (targetSlot == null || targetSlot.IsOccupied)
                {
                    _logger.LogWarning("Requested slot {SlotId} is unavailable for vehicle {VehicleNumber}", slotId, vehicleNumber);
                    return false;
                }
            }
            else
            {
                targetSlot = await _slotService.GetAvailableSlotAsync();
                if (targetSlot == null)
                {
                    _logger.LogWarning("No available slot found for vehicle {VehicleNumber}", vehicleNumber);
                    return false;
                }
            }

            var record = new ParkingRecord
            {
                VehicleNumber = vehicleNumber,
                OwnerName = ownerName,
                ParkingSlotId = targetSlot.Id,
                EntryTime = DateTime.Now,
                IsCompleted = false
            };

            _context.ParkingRecords.Add(record);
            await _slotService.UpdateSlotStatusAsync(targetSlot.Id, true);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle {VehicleNumber} assigned to slot {SlotNumber}", vehicleNumber, targetSlot.SlotNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record vehicle entry for {VehicleNumber}", vehicleNumber);
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

    public async Task<bool> RecordVehicleExitAsync(int recordId)
    {
        try
        {
            _logger.LogInformation("Recording vehicle exit for record {RecordId}", recordId);
            var record = await _context.ParkingRecords.FindAsync(recordId);
            if (record == null)
            {
                _logger.LogWarning("Record {RecordId} was not found", recordId);
                return false;
            }

            if (record.IsCompleted)
            {
                _logger.LogWarning("Record {RecordId} was already completed", recordId);
                return false;
            }

            record.ExitTime = DateTime.Now;
            record.Duration = record.ExitTime - record.EntryTime;
            record.IsCompleted = true;

            await _slotService.UpdateSlotStatusAsync(record.ParkingSlotId, false);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle exit recorded for record {RecordId}", recordId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record vehicle exit for record {RecordId}", recordId);
            return false;
        }
    }
}
