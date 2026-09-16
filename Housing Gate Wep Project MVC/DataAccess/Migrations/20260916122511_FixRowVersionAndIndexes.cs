using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHousing.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Properties",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_VerificationStatus",
                table: "StudentProfiles",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerProfiles_UserId",
                table: "OwnerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OwnerProfiles_VerificationStatus",
                table: "OwnerProfiles",
                column: "VerificationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_VerificationStatus",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_OwnerProfiles_UserId",
                table: "OwnerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_OwnerProfiles_VerificationStatus",
                table: "OwnerProfiles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Properties");
        }
    }
}
