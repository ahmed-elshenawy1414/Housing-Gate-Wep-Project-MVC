using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHousing.Migrations
{
    /// <inheritdoc />
    public partial class AddHousingRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HousingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentProfileId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Governorate = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    University = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PropertyType = table.Column<int>(type: "int", nullable: false),
                    BudgetMin = table.Column<int>(type: "int", nullable: false),
                    BudgetMax = table.Column<int>(type: "int", nullable: false),
                    Bedrooms = table.Column<int>(type: "int", nullable: false),
                    Bathrooms = table.Column<int>(type: "int", nullable: false),
                    IsFurnished = table.Column<bool>(type: "bit", nullable: false),
                    PetAllowed = table.Column<bool>(type: "bit", nullable: false),
                    PreferredGender = table.Column<int>(type: "int", nullable: false),
                    MoveInDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinLeaseMonths = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingRequests", x => x.Id);
                    table.CheckConstraint("CK_HousingRequest_Bathrooms_Range", "[Bathrooms] >= 0 AND [Bathrooms] <= 10");
                    table.CheckConstraint("CK_HousingRequest_Bedrooms_Range", "[Bedrooms] >= 1 AND [Bedrooms] <= 20");
                    table.CheckConstraint("CK_HousingRequest_BudgetMax_NonNegative", "[BudgetMax] >= 0");
                    table.CheckConstraint("CK_HousingRequest_BudgetMin_NonNegative", "[BudgetMin] >= 0");
                    table.ForeignKey(
                        name: "FK_HousingRequests_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HousingRequestAmenities",
                columns: table => new
                {
                    AmenitiesId = table.Column<int>(type: "int", nullable: false),
                    HousingRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingRequestAmenities", x => new { x.AmenitiesId, x.HousingRequestId });
                    table.ForeignKey(
                        name: "FK_HousingRequestAmenities_Amenities_AmenitiesId",
                        column: x => x.AmenitiesId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HousingRequestAmenities_HousingRequests_HousingRequestId",
                        column: x => x.HousingRequestId,
                        principalTable: "HousingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HousingRequestOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HousingRequestId = table.Column<int>(type: "int", nullable: false),
                    OffererUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OfferedPropertyId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingRequestOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HousingRequestOffers_AspNetUsers_OffererUserId",
                        column: x => x.OffererUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HousingRequestOffers_HousingRequests_HousingRequestId",
                        column: x => x.HousingRequestId,
                        principalTable: "HousingRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HousingRequestOffers_Properties_OfferedPropertyId",
                        column: x => x.OfferedPropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_AllowedGender",
                table: "Properties",
                column: "AllowedGender");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequestAmenities_HousingRequestId",
                table: "HousingRequestAmenities",
                column: "HousingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequestOffers_HousingRequestId_Status",
                table: "HousingRequestOffers",
                columns: new[] { "HousingRequestId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequestOffers_OfferedPropertyId",
                table: "HousingRequestOffers",
                column: "OfferedPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequestOffers_OffererUserId",
                table: "HousingRequestOffers",
                column: "OffererUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequests_City",
                table: "HousingRequests",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequests_CreatedAt",
                table: "HousingRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequests_IsActive_IsClosed",
                table: "HousingRequests",
                columns: new[] { "IsActive", "IsClosed" });

            migrationBuilder.CreateIndex(
                name: "IX_HousingRequests_StudentProfileId",
                table: "HousingRequests",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HousingRequestAmenities");

            migrationBuilder.DropTable(
                name: "HousingRequestOffers");

            migrationBuilder.DropTable(
                name: "HousingRequests");

            migrationBuilder.DropIndex(
                name: "IX_Properties_AllowedGender",
                table: "Properties");
        }
    }
}
