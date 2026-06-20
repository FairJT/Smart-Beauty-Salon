using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonOS.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "JobSeekerEnabled",
                table: "ClientProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobSeekerEnabled",
                table: "ClientProfiles");
        }
    }
}
