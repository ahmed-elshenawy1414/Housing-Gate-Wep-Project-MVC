using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHousing.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_BookingIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Stays",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rooms",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PropertyApplications",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Beds",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Room_AvailableBeds_Range",
                table: "Rooms",
                sql: "[AvailableBeds] >= 0 AND [AvailableBeds] <= [NumberOfBeds]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Room_NumberOfBeds_Range",
                table: "Rooms",
                sql: "[NumberOfBeds] >= 0 AND [NumberOfBeds] <= 20");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Room_RentPerMonth_NonNegative",
                table: "Rooms",
                sql: "[RentPerMonth] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyApplications_RoomId_StudentProfileId",
                table: "PropertyApplications",
                columns: new[] { "RoomId", "StudentProfileId" },
                unique: true,
                filter: "[Status] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Room_AvailableBeds_Range",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Room_NumberOfBeds_Range",
                table: "Rooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Room_RentPerMonth_NonNegative",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_PropertyApplications_RoomId_StudentProfileId",
                table: "PropertyApplications");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Stays");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PropertyApplications");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Beds");
        }
    }
}
