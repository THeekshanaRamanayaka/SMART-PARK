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

    public async Task<List<ParkingSlot>> GetAvailableSlotsAsync()
    {
        try
        {
            _logger.LogInformation("Loading all available parking slots");
            return await _context.ParkingSlots
                .Where(s => !s.IsOccupied)
                .OrderBy(s => s.SlotNumber)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load available parking slots");
            return new List<ParkingSlot>();
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

    /// <summary>
    /// Server-side paginated view over parking slots, prioritizing availability.
    /// Available (empty) slots appear first, followed by occupied slots, sorted by
    /// slot number within each group. Supports efficient browsing of large slot
    /// inventories without materializing the entire list.
    /// </summary>
    public async Task<PagedResult<ParkingSlot>> GetSlotsPaginatedAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        try
        {
            // Prioritize available slots (IsOccupied == false) first, then occupied.
            var query = _context.ParkingSlots
                .AsNoTracking()
                .OrderBy(s => s.IsOccupied)
                .ThenBy(s => s.SlotNumber);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation(
                "Loaded parking slots page {Page} (size {PageSize}, total {Total}, prioritized by availability)",
                page, pageSize, totalCount);

            return new PagedResult<ParkingSlot>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load parking slots page {Page}", page);
            return PagedResult<ParkingSlot>.Empty(page, pageSize);
        }
    }
}
