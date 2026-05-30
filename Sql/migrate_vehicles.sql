-- Backfill script: create Vehicles from existing ParkingRecords and set ParkingRecords.VehicleId
-- Run this after applying EF migration that adds Vehicles and VehicleId.

START TRANSACTION;

-- Insert one vehicle row per distinct normalized plate using the latest owner info
INSERT INTO Vehicles (Plate, PlateNormalized, OwnerName, LastSeen, CreatedAt, UpdatedAt)
SELECT pr.VehicleNumber AS Plate,
             latest.PlateNormalized AS PlateNormalized,
             pr.OwnerName,
             latest.LastSeen AS LastSeen,
             NOW() AS CreatedAt,
             NOW() AS UpdatedAt
FROM ParkingRecords pr
INNER JOIN (
    SELECT UPPER(TRIM(VehicleNumber)) AS PlateNormalized, MAX(EntryTime) AS LastSeen
    FROM ParkingRecords
    GROUP BY UPPER(TRIM(VehicleNumber))
) latest ON UPPER(TRIM(pr.VehicleNumber)) = latest.PlateNormalized AND pr.EntryTime = latest.LastSeen;

-- Backfill VehicleId on ParkingRecords by matching normalized plate
UPDATE ParkingRecords p
JOIN Vehicles v ON UPPER(TRIM(p.VehicleNumber)) = v.PlateNormalized
SET p.VehicleId = v.Id;

COMMIT;
