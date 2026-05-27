START TRANSACTION;

DELETE FROM ParkingRecords
WHERE VehicleNumber IN (
    'E2E-HISTORY',
    'E2E-ACTIVE',
    'E2E-UI-ENTRY'
);

DELETE FROM ParkingSlots
WHERE SlotNumber IN ('E2E-01', 'E2E-02', 'E2E-03');

INSERT INTO ParkingSlots (SlotNumber, IsOccupied, LastUpdated)
VALUES
    ('E2E-01', 0, NOW()),
    ('E2E-02', 1, NOW()),
    ('E2E-03', 0, NOW());

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'E2E-HISTORY',
    'Completed Test Driver',
    Id,
    DATE_SUB(NOW(), INTERVAL 2 DAY) - INTERVAL 3 HOUR,
    DATE_SUB(NOW(), INTERVAL 2 DAY) - INTERVAL 1 HOUR - INTERVAL 25 MINUTE,
    '01:35:00',
    1
FROM ParkingSlots
WHERE SlotNumber = 'E2E-01';

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'E2E-ACTIVE',
    'Active Test Driver',
    Id,
    NOW() - INTERVAL 50 MINUTE,
    NULL,
    NULL,
    0
FROM ParkingSlots
WHERE SlotNumber = 'E2E-02';

COMMIT;

SELECT
    r.VehicleNumber,
    r.OwnerName,
    s.SlotNumber,
    r.EntryTime,
    r.ExitTime,
    r.Duration,
    r.IsCompleted,
    s.IsOccupied
FROM ParkingRecords r
JOIN ParkingSlots s ON s.Id = r.ParkingSlotId
WHERE r.VehicleNumber LIKE 'E2E-%'
ORDER BY r.IsCompleted, r.VehicleNumber;
