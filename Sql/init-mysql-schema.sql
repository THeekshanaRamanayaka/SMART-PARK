CREATE TABLE IF NOT EXISTS AppUsers (
    Id INT NOT NULL AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PasswordHash LONGTEXT NOT NULL,
    Role VARCHAR(20) NOT NULL,
    IsActive TINYINT(1) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_AppUsers_Username (Username),
    UNIQUE KEY IX_AppUsers_Email (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS ParkingSlots (
    Id INT NOT NULL AUTO_INCREMENT,
    SlotNumber VARCHAR(10) NOT NULL,
    IsOccupied TINYINT(1) NOT NULL,
    LastUpdated DATETIME(6) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_ParkingSlots_SlotNumber (SlotNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS ParkingRecords (
    Id INT NOT NULL AUTO_INCREMENT,
    VehicleNumber VARCHAR(20) NOT NULL,
    OwnerName VARCHAR(100) NOT NULL,
    ParkingSlotId INT NOT NULL,
    EntryTime DATETIME(6) NOT NULL,
    ExitTime DATETIME(6) NULL,
    Duration TIME(6) NULL,
    IsCompleted TINYINT(1) NOT NULL,
    PRIMARY KEY (Id),
    KEY IX_ParkingRecords_ParkingSlotId (ParkingSlotId),
    CONSTRAINT FK_ParkingRecords_ParkingSlots_ParkingSlotId
        FOREIGN KEY (ParkingSlotId) REFERENCES ParkingSlots (Id)
        ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
