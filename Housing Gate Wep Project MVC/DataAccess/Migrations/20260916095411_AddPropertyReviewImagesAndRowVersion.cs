using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentHousing.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyReviewImagesAndRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UserReviews",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PropertyReviews",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "PropertyReviewImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyReviewId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyReviewImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyReviewImages_PropertyReviews_PropertyReviewId",
                        column: x => x.PropertyReviewId,
                        principalTable: "PropertyReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyReviewImages_PropertyReviewId",
                table: "PropertyReviewImages",
                column: "PropertyReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyReviewImages");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserReviews");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PropertyReviews");
        }
    }
}
