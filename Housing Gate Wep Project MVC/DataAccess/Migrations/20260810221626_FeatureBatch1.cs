using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHousing.Migrations
{
    /// <inheritdoc />
    public partial class FeatureBatch1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerificationDocumentUrl",
                table: "StudentProfiles",
                newName: "UniversityIdDocumentUrl");

            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "StudentProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "StudentProfiles",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "StudentProfiles",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdDocumentUrl",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationRejectReason",
                table: "StudentProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvailableBeds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BathroomType",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBeds",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "PropertyImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "PropertyImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFromUniversityKm",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Properties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsModifiedSinceApproval",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Properties",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "University",
                table: "Properties",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    StudentProfileId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beds_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Beds_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyAmenities",
                columns: table => new
                {
                    AmenitiesId = table.Column<int>(type: "int", nullable: false),
                    PropertiesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAmenities", x => new { x.AmenitiesId, x.PropertiesId });
                    table.ForeignKey(
                        name: "FK_PropertyAmenities_Amenities_AmenitiesId",
                        column: x => x.AmenitiesId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyAmenities_Properties_PropertiesId",
                        column: x => x.PropertiesId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomAmenities",
                columns: table => new
                {
                    AmenitiesId = table.Column<int>(type: "int", nullable: false),
                    RoomsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAmenities", x => new { x.AmenitiesId, x.RoomsId });
                    table.ForeignKey(
                        name: "FK_RoomAmenities_Amenities_AmenitiesId",
                        column: x => x.AmenitiesId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomAmenities_Rooms_RoomsId",
                        column: x => x.RoomsId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_RoomId",
                table: "PropertyImages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PublicId",
                table: "Properties",
                column: "PublicId",
                unique: true,
                filter: "[PublicId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Name",
                table: "Amenities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beds_RoomId_IsAvailable",
                table: "Beds",
                columns: new[] { "RoomId", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_Beds_StudentProfileId",
                table: "Beds",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyAmenities_PropertiesId",
                table: "PropertyAmenities",
                column: "PropertiesId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAmenities_RoomsId",
                table: "RoomAmenities",
                column: "RoomsId");

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Name",
                table: "Universities",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyImages_Rooms_RoomId",
                table: "PropertyImages",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            // Backfill a public ID (APT-00001...) for properties that already exist.
            migrationBuilder.Sql(
                "UPDATE [Properties] SET [PublicId] = " +
                "CONCAT('APT-', RIGHT(CONCAT('00000', CAST([Id] AS NVARCHAR(10))), 5)) " +
                "WHERE [PublicId] IS NULL");

            // Backfill sensible bed counts for existing rooms (shared rooms get 2 beds, others 1).
            migrationBuilder.Sql(
                "UPDATE [Rooms] SET [NumberOfBeds] = CASE WHEN [RoomType] = 1 THEN 2 ELSE 1 END " +
                "WHERE [NumberOfBeds] = 0");
            migrationBuilder.Sql(
                "UPDATE [Rooms] SET [AvailableBeds] = [NumberOfBeds] " +
                "WHERE [AvailableBeds] = 0 AND [IsAvailable] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyImages_Rooms_RoomId",
                table: "PropertyImages");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "PropertyAmenities");

            migrationBuilder.DropTable(
                name: "RoomAmenities");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImages_RoomId",
                table: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_Properties_PublicId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "District",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "NationalIdDocumentUrl",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "VerificationRejectReason",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "AvailableBeds",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BathroomType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "NumberOfBeds",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "PropertyImages");

            migrationBuilder.DropColumn(
                name: "DistanceFromUniversityKm",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsModifiedSinceApproval",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "University",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "UniversityIdDocumentUrl",
                table: "StudentProfiles",
                newName: "VerificationDocumentUrl");
        }
    }
}
