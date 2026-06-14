using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSalon.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminTheme",
                table: "Salons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminTheme",
                table: "Salons");
        }
    }
}
