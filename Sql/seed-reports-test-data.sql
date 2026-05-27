PRAGMA foreign_keys = ON;

BEGIN TRANSACTION;

DELETE FROM ParkingRecords
WHERE VehicleNumber IN ('TEST-1001', 'TEST-1002', 'TEST-1003', 'TEST-ACTIVE');

INSERT OR IGNORE INTO ParkingSlots (SlotNumber, IsOccupied, LastUpdated)
VALUES
    ('T-01', 0, datetime('now')),
    ('T-02', 0, datetime('now')),
    ('T-03', 0, datetime('now')),
    ('T-04', 0, datetime('now'));

UPDATE ParkingSlots
SET IsOccupied = 0,
    LastUpdated = datetime('now')
WHERE SlotNumber IN ('T-01', 'T-02', 'T-03', 'T-04');

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'TEST-1001',
    'Test Driver One',
    Id,
    datetime('now', '-5 hours'),
    datetime('now', '-3 hours', '-30 minutes'),
    '01:30:00',
    1
FROM ParkingSlots
WHERE SlotNumber = 'T-01';

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'TEST-1002',
    'Test Driver Two',
    Id,
    datetime('now', '-2 days', '-4 hours'),
    datetime('now', '-2 days', '-1 hours', '-45 minutes'),
    '02:15:00',
    1
FROM ParkingSlots
WHERE SlotNumber = 'T-02';

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'TEST-1003',
    'Test Driver Three',
    Id,
    datetime('now', '-1 days', '-7 hours'),
    datetime('now', '-1 days', '-3 hours', '-20 minutes'),
    '03:40:00',
    1
FROM ParkingSlots
WHERE SlotNumber = 'T-03';

INSERT INTO ParkingRecords
    (VehicleNumber, OwnerName, ParkingSlotId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT
    'TEST-ACTIVE',
    'Active Test Driver',
    Id,
    datetime('now', '-45 minutes'),
    NULL,
    NULL,
    0
FROM ParkingSlots
WHERE SlotNumber = 'T-04';

UPDATE ParkingSlots
SET IsOccupied = 1,
    LastUpdated = datetime('now')
WHERE SlotNumber = 'T-04';

COMMIT;

SELECT
    r.VehicleNumber,
    r.OwnerName,
    s.SlotNumber,
    r.EntryTime,
    r.ExitTime,
    r.Duration,
    r.IsCompleted
FROM ParkingRecords r
JOIN ParkingSlots s ON s.Id = r.ParkingSlotId
WHERE r.VehicleNumber LIKE 'TEST-%'
ORDER BY r.IsCompleted DESC, r.ExitTime DESC;
