using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonOS.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateSavedSalonToSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SavedSalons_UserId_SalonId",
                table: "SavedSalons");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "SavedSalons");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "SavedSalons",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons",
                columns: new[] { "UserId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "SavedSalons");

            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "SavedSalons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SavedSalons_UserId_SalonId",
                table: "SavedSalons",
                columns: new[] { "UserId", "SalonId" },
                unique: true);
        }
    }
}
