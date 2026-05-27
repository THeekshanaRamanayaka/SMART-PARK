using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPark.Data;
using SmartPark.Models;

namespace SmartPark.Services;

public class ParkingSlotService
{
    private readonly SmartParkDbContext _context;
    private readonly ILogger<ParkingSlotService> _logger;

    public ParkingSlotService(SmartParkDbContext context, ILogger<ParkingSlotService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ParkingSlot>> GetAllSlotsAsync()
    {
        try
        {
            _logger.LogInformation("Loading all parking slots");
            return await _context.ParkingSlots.OrderBy(s => s.SlotNumber).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load parking slots");
            return new List<ParkingSlot>();
        }
    }

    public async Task<ParkingSlot?> GetSlotByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Loading parking slot {SlotId}", id);
            return await _context.ParkingSlots.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load parking slot {SlotId}", id);
            return null;
        }
    }

    public async Task<ParkingSlot?> GetAvailableSlotAsync()
    {
        try
        {
            _logger.LogInformation("Loading next available parking slot");
            return await _context.ParkingSlots
                .Where(s => !s.IsOccupied)
                .OrderBy(s => s.SlotNumber)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load available parking slot");
            return null;
        }
    }

    public async Task<bool> AddSlotAsync(ParkingSlot slot)
    {
        try
        {
            slot.LastUpdated = DateTime.Now;
            _logger.LogInformation("Adding parking slot {SlotNumber}", slot.SlotNumber);
            _context.ParkingSlots.Add(slot);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add parking slot {SlotNumber}", slot.SlotNumber);
            return false;
        }
    }

    public async Task<bool> UpdateSlotAsync(ParkingSlot slot)
    {
        try
        {
            slot.LastUpdated = DateTime.Now;
            _logger.LogInformation("Updating parking slot {SlotId}", slot.Id);
            _context.ParkingSlots.Update(slot);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update parking slot {SlotId}", slot.Id);
            return false;
        }
    }

    public async Task<bool> DeleteSlotAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting parking slot {SlotId}", id);
            var slot = await _context.ParkingSlots.FindAsync(id);
            if (slot == null) return false;

            _context.ParkingSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete parking slot {SlotId}", id);
            return false;
        }
    }

    public async Task<bool> UpdateSlotStatusAsync(int slotId, bool isOccupied)
    {
        try
        {
            _logger.LogInformation("Updating parking slot {SlotId} occupied status to {IsOccupied}", slotId, isOccupied);
            var slot = await _context.ParkingSlots.FindAsync(slotId);
            if (slot == null) return false;

            slot.IsOccupied = isOccupied;
            slot.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update occupied status for parking slot {SlotId}", slotId);
            return false;
        }
    }

    public async Task<int> GetAvailableSlotsCountAsync()
    {
        try
        {
            _logger.LogInformation("Counting available parking slots");
            return await _context.ParkingSlots.CountAsync(s => !s.IsOccupied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count available parking slots");
            return 0;
        }
    }

    public async Task<int> GetOccupiedSlotsCountAsync()
    {
        try
        {
            _logger.LogInformation("Counting occupied parking slots");
            return await _context.ParkingSlots.CountAsync(s => s.IsOccupied);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count occupied parking slots");
            return 0;
        }
    }
}
