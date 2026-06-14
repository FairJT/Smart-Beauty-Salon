using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSalon.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistSkill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Skill",
                table: "Artists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skill",
                table: "Artists");
        }
    }
}
