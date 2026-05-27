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

    public async Task<bool> RecordVehicleEntryAsync(string vehicleNumber, string ownerName)
    {
        try
        {
            _logger.LogInformation("Recording vehicle entry for {VehicleNumber}", vehicleNumber);
            var availableSlot = await _slotService.GetAvailableSlotAsync();
            if (availableSlot == null)
            {
                _logger.LogWarning("No available slot found for vehicle {VehicleNumber}", vehicleNumber);
                return false;
            }

            var record = new ParkingRecord
            {
                VehicleNumber = vehicleNumber,
                OwnerName = ownerName,
                ParkingSlotId = availableSlot.Id,
                EntryTime = DateTime.Now,
                IsCompleted = false
            };

            _context.ParkingRecords.Add(record);
            await _slotService.UpdateSlotStatusAsync(availableSlot.Id, true);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Vehicle {VehicleNumber} assigned to slot {SlotNumber}", vehicleNumber, availableSlot.SlotNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record vehicle entry for {VehicleNumber}", vehicleNumber);
            return false;
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
