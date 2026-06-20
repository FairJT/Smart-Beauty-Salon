using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonOS.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonRatingDenorm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "RatingSum",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RatingSum",
                table: "Tenants");
        }
    }
}
