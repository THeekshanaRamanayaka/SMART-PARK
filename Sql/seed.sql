-- SmartPark Test Data
-- Run this after schema.sql on a fresh database.
--
-- Credentials:
--   admin      / Admin@123
--   operator1  / Operator@123
--   operator2  / Operator@123

START TRANSACTION;

-- ─── Users ───────────────────────────────────────────────────────────────────

INSERT INTO AppUsers (Username, FullName, Email, PasswordHash, Role, IsActive, CreatedAt) VALUES
('admin',     'Kasun Perera',    'admin@smartpark.lk',    '$2b$11$PTKcd5MZGaY3g5g054xqk.9lJFNYGPjnaeslZeT.I7yoGA.1fRfeu', 'Admin', 1, NOW()),
('operator1', 'Nuwan Fernando',  'nuwan@smartpark.lk',    '$2b$11$vFN1fzTypSiuJYfTD8MvSOuaVAf9gVaMXx8It2RGM47GRd0bBcN2C', 'User',  1, NOW()),
('operator2', 'Priyanka Perera', 'priyanka@smartpark.lk', '$2b$11$Td9Wc4So1kgX5JAQIXTwQ.FZ2Q//pg3XpHpQW0KZE55z9nfjgMnVC', 'User',  1, NOW());

-- ─── Parking Slots (Zone A: 10 slots, Zone B: 10 slots) ──────────────────────
-- Slots marked IsOccupied=1 match the active sessions inserted below.

INSERT INTO ParkingSlots (SlotNumber, IsOccupied, LastUpdated) VALUES
('A-01', 0, NOW()), ('A-02', 0, NOW()), ('A-03', 0, NOW()), ('A-04', 0, NOW()), ('A-05', 0, NOW()),
('A-06', 1, NOW()), ('A-07', 0, NOW()), ('A-08', 0, NOW()), ('A-09', 0, NOW()), ('A-10', 0, NOW()),
('B-01', 0, NOW()), ('B-02', 0, NOW()), ('B-03', 1, NOW()), ('B-04', 1, NOW()), ('B-05', 0, NOW()),
('B-06', 0, NOW()), ('B-07', 0, NOW()), ('B-08', 0, NOW()), ('B-09', 0, NOW()), ('B-10', 0, NOW());

-- ─── Vehicles ─────────────────────────────────────────────────────────────────

INSERT INTO Vehicles (Plate, PlateNormalized, OwnerName, LastSeen, CreatedAt, UpdatedAt) VALUES
('CAG-4521',   'CAG-4521',   'Saman Perera',          DATE_SUB(NOW(), INTERVAL 1 DAY),  NOW(), NOW()),
('CAB-3312',   'CAB-3312',   'Kamala Silva',           DATE_SUB(NOW(), INTERVAL 2 DAY),  NOW(), NOW()),
('WP-AB-1234', 'WP-AB-1234', 'Dilani Rajapaksa',      DATE_SUB(NOW(), INTERVAL 3 DAY),  NOW(), NOW()),
('WP-CD-5678', 'WP-CD-5678', 'Mahesh Jayawardena',    DATE_SUB(NOW(), INTERVAL 1 DAY),  NOW(), NOW()),
('CP-EF-9012', 'CP-EF-9012', 'Priyanka Weerasekara',  DATE_SUB(NOW(), INTERVAL 4 DAY),  NOW(), NOW()),
('SP-GH-3456', 'SP-GH-3456', 'Kasun Bandara',         DATE_SUB(NOW(), INTERVAL 3 DAY),  NOW(), NOW()),
('NW-IJ-7890', 'NW-IJ-7890', 'Thilini Dissanayake',   NOW(),                            NOW(), NOW()),
('NC-KL-2345', 'NC-KL-2345', 'Roshan Wickramasinghe', NOW(),                            NOW(), NOW()),
('EP-MN-6789', 'EP-MN-6789', 'Sanduni Amarasinghe',   NOW(),                            NOW(), NOW()),
('WP-OP-0123', 'WP-OP-0123', 'Chamara Gunawardena',   DATE_SUB(NOW(), INTERVAL 5 DAY),  NOW(), NOW());

-- ─── Completed Parking Records ────────────────────────────────────────────────

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'CAG-4521', 'Saman Perera',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-01'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'CAG-4521'),
    DATE_SUB(NOW(), INTERVAL 6 DAY) - INTERVAL 3 HOUR,
    DATE_SUB(NOW(), INTERVAL 6 DAY) - INTERVAL 1 HOUR - INTERVAL 10 MINUTE,
    '01:50:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'CAB-3312', 'Kamala Silva',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-02'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'CAB-3312'),
    DATE_SUB(NOW(), INTERVAL 5 DAY) - INTERVAL 2 HOUR - INTERVAL 30 MINUTE,
    DATE_SUB(NOW(), INTERVAL 5 DAY) - INTERVAL 45 MINUTE,
    '01:45:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'WP-AB-1234', 'Dilani Rajapaksa',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-03'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'WP-AB-1234'),
    DATE_SUB(NOW(), INTERVAL 4 DAY) - INTERVAL 8 HOUR,
    DATE_SUB(NOW(), INTERVAL 4 DAY) - INTERVAL 4 HOUR - INTERVAL 50 MINUTE,
    '03:10:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'WP-CD-5678', 'Mahesh Jayawardena',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-01'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'WP-CD-5678'),
    DATE_SUB(NOW(), INTERVAL 4 DAY) - INTERVAL 1 HOUR,
    DATE_SUB(NOW(), INTERVAL 4 DAY) - INTERVAL 5 MINUTE,
    '00:55:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'CP-EF-9012', 'Priyanka Weerasekara',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-02'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'CP-EF-9012'),
    DATE_SUB(NOW(), INTERVAL 3 DAY) - INTERVAL 2 HOUR - INTERVAL 30 MINUTE,
    DATE_SUB(NOW(), INTERVAL 3 DAY),
    '02:30:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'SP-GH-3456', 'Kasun Bandara',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-04'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'SP-GH-3456'),
    DATE_SUB(NOW(), INTERVAL 3 DAY) - INTERVAL 1 HOUR - INTERVAL 20 MINUTE,
    DATE_SUB(NOW(), INTERVAL 3 DAY),
    '01:20:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'NW-IJ-7890', 'Thilini Dissanayake',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-05'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'NW-IJ-7890'),
    DATE_SUB(NOW(), INTERVAL 2 DAY) - INTERVAL 45 MINUTE,
    DATE_SUB(NOW(), INTERVAL 2 DAY),
    '00:45:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'CAG-4521', 'Saman Perera',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-07'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'CAG-4521'),
    DATE_SUB(NOW(), INTERVAL 2 DAY) - INTERVAL 1 HOUR - INTERVAL 5 MINUTE,
    DATE_SUB(NOW(), INTERVAL 2 DAY),
    '01:05:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'CAB-3312', 'Kamala Silva',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-08'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'CAB-3312'),
    DATE_SUB(NOW(), INTERVAL 1 DAY) - INTERVAL 2 HOUR - INTERVAL 45 MINUTE,
    DATE_SUB(NOW(), INTERVAL 1 DAY),
    '02:45:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'WP-CD-5678', 'Mahesh Jayawardena',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-05'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'WP-CD-5678'),
    DATE_SUB(NOW(), INTERVAL 1 DAY) - INTERVAL 2 HOUR - INTERVAL 30 MINUTE,
    DATE_SUB(NOW(), INTERVAL 1 DAY),
    '02:30:00', 1;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'WP-OP-0123', 'Chamara Gunawardena',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-06'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'WP-OP-0123'),
    DATE_SUB(NOW(), INTERVAL 75 MINUTE),
    NOW(),
    '01:15:00', 1;

-- ─── Active Parking Records (currently parked) ────────────────────────────────

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'NC-KL-2345', 'Roshan Wickramasinghe',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'A-06'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'NC-KL-2345'),
    DATE_SUB(NOW(), INTERVAL 70 MINUTE),
    NULL, NULL, 0;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'NW-IJ-7890', 'Thilini Dissanayake',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-03'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'NW-IJ-7890'),
    DATE_SUB(NOW(), INTERVAL 35 MINUTE),
    NULL, NULL, 0;

INSERT INTO ParkingRecords (VehicleNumber, OwnerName, ParkingSlotId, VehicleId, EntryTime, ExitTime, Duration, IsCompleted)
SELECT 'EP-MN-6789', 'Sanduni Amarasinghe',
    (SELECT Id FROM ParkingSlots WHERE SlotNumber = 'B-04'),
    (SELECT Id FROM Vehicles     WHERE PlateNormalized = 'EP-MN-6789'),
    DATE_SUB(NOW(), INTERVAL 20 MINUTE),
    NULL, NULL, 0;

COMMIT;
