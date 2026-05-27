SELECT 'Dashboard counts' AS section;

SELECT
    (SELECT COUNT(*) FROM ParkingSlots) AS TotalSlots,
    (SELECT COUNT(*) FROM ParkingSlots WHERE IsOccupied = 0) AS AvailableSlots,
    (SELECT COUNT(*) FROM ParkingSlots WHERE IsOccupied = 1) AS OccupiedSlots,
    (SELECT COUNT(*) FROM ParkingRecords WHERE IsCompleted = 0) AS ActiveSessions,
    (SELECT COUNT(*) FROM ParkingRecords WHERE IsCompleted = 1) AS CompletedSessions;

SELECT 'E2E records' AS section;

SELECT
    r.Id,
    r.VehicleNumber,
    r.OwnerName,
    s.SlotNumber,
    r.EntryTime,
    r.ExitTime,
    r.Duration,
    CASE r.IsCompleted WHEN 1 THEN 'Completed' ELSE 'Active' END AS Status,
    CASE s.IsOccupied WHEN 1 THEN 'Occupied' ELSE 'Available' END AS SlotStatus
FROM ParkingRecords r
JOIN ParkingSlots s ON s.Id = r.ParkingSlotId
WHERE r.VehicleNumber LIKE 'E2E-%'
ORDER BY r.EntryTime DESC;

SELECT 'UI entry check' AS section;

SELECT
    r.Id,
    r.VehicleNumber,
    r.OwnerName,
    s.SlotNumber,
    r.EntryTime,
    r.ExitTime,
    r.Duration,
    CASE r.IsCompleted WHEN 1 THEN 'Completed' ELSE 'Active' END AS Status
FROM ParkingRecords r
JOIN ParkingSlots s ON s.Id = r.ParkingSlotId
WHERE r.VehicleNumber = 'E2E-UI-ENTRY'
ORDER BY r.Id DESC;
