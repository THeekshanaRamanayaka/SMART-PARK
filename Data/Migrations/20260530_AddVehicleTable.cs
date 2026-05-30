using Microsoft.EntityFrameworkCore.Migrations;

namespace SmartPark.Data.Migrations
{
    public partial class AddVehicleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS Vehicles (
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            migrationBuilder.Sql("ALTER TABLE ParkingRecords ADD COLUMN IF NOT EXISTS VehicleId INT NULL;");
            migrationBuilder.Sql("ALTER TABLE ParkingRecords ADD INDEX IF NOT EXISTS IX_ParkingRecords_VehicleId (VehicleId);");
            migrationBuilder.Sql("ALTER TABLE ParkingRecords ADD CONSTRAINT IF NOT EXISTS FK_ParkingRecords_Vehicles_VehicleId FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id) ON DELETE SET NULL ON UPDATE CASCADE;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE ParkingRecords DROP FOREIGN KEY IF EXISTS FK_ParkingRecords_Vehicles_VehicleId;");
            migrationBuilder.Sql("ALTER TABLE ParkingRecords DROP INDEX IF EXISTS IX_ParkingRecords_VehicleId;");
            migrationBuilder.Sql("ALTER TABLE ParkingRecords DROP COLUMN IF EXISTS VehicleId;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS Vehicles;");
        }
    }
}
