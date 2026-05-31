-- SmartPark Database Schema
-- Run this once on a fresh database before starting the application.

START TRANSACTION;

CREATE TABLE IF NOT EXISTS AppUsers (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Username    VARCHAR(50)  NOT NULL,
    FullName    VARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NOT NULL,
    PasswordHash TEXT        NOT NULL,
    Role        VARCHAR(20)  NOT NULL DEFAULT 'User',
    IsActive    TINYINT(1)   NOT NULL DEFAULT 1,
    CreatedAt   DATETIME(6)  NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_AppUsers_Username (Username),
    UNIQUE KEY IX_AppUsers_Email    (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id          INT          NOT NULL AUTO_INCREMENT,
    AppUserId   INT          NOT NULL,
    Token       VARCHAR(200) NOT NULL,
    ExpiresAt   DATETIME(6)  NOT NULL,
    CreatedAt   DATETIME(6)  NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    Revoked     TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    CONSTRAINT FK_RefreshTokens_AppUsers
        FOREIGN KEY (AppUserId) REFERENCES AppUsers (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ParkingSlots (
    Id          INT         NOT NULL AUTO_INCREMENT,
    SlotNumber  VARCHAR(10) NOT NULL,
    IsOccupied  TINYINT(1)  NOT NULL DEFAULT 0,
    LastUpdated DATETIME(6) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_ParkingSlots_SlotNumber (SlotNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS Vehicles (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Plate            VARCHAR(20)  NOT NULL,
    PlateNormalized  VARCHAR(20)  NOT NULL,
    OwnerName        VARCHAR(100) NULL,
    AppUserId        INT          NULL,
    LastSeen         DATETIME     NULL,
    CreatedAt        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    RowVersion       BINARY(8)    NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_Vehicles_PlateNormalized (PlateNormalized)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS ParkingRecords (
    Id            INT          NOT NULL AUTO_INCREMENT,
    VehicleNumber VARCHAR(20)  NOT NULL,
    OwnerName     VARCHAR(100) NOT NULL,
    ParkingSlotId INT          NOT NULL,
    VehicleId     INT          NULL,
    EntryTime     DATETIME(6)  NOT NULL,
    ExitTime      DATETIME(6)  NULL,
    Duration      TIME(6)      NULL,
    IsCompleted   TINYINT(1)   NOT NULL DEFAULT 0,
    PRIMARY KEY (Id),
    KEY IX_ParkingRecords_ParkingSlotId (ParkingSlotId),
    KEY IX_ParkingRecords_VehicleId     (VehicleId),
    CONSTRAINT FK_ParkingRecords_ParkingSlots_ParkingSlotId
        FOREIGN KEY (ParkingSlotId) REFERENCES ParkingSlots (Id) ON DELETE RESTRICT,
    CONSTRAINT FK_ParkingRecords_Vehicles_VehicleId
        FOREIGN KEY (VehicleId) REFERENCES Vehicles (Id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

COMMIT;
