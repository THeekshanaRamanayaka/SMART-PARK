-- Create Vehicles table and add VehicleId FK to ParkingRecords
-- Run this before migrate_vehicles.sql

START TRANSACTION;

CREATE TABLE IF NOT EXISTS Vehicles (
  Id INT AUTO_INCREMENT PRIMARY KEY,
  Plate VARCHAR(20) NOT NULL,
  PlateNormalized VARCHAR(20) NOT NULL,
  OwnerName VARCHAR(100),
  AppUserId INT NULL,
  LastSeen DATETIME NULL,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  RowVersion BINARY(8),
  UNIQUE KEY IX_Vehicles_PlateNormalized (PlateNormalized)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

ALTER TABLE ParkingRecords
  ADD COLUMN VehicleId INT NULL;

ALTER TABLE ParkingRecords
  ADD INDEX IX_ParkingRecords_VehicleId (VehicleId),
  ADD CONSTRAINT FK_ParkingRecords_Vehicles_VehicleId FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE SET NULL ON UPDATE CASCADE;

COMMIT;
