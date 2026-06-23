using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace SalonOS.Infrastructure.Migrations
{
    public partial class AddOutboxMessageAndCheckedInAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create OutboxMessages table
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dispatched = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            // Add CheckedInAt column to Bookings table
            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove CheckedInAt column
            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Bookings");

            // Drop OutboxMessages table
            migrationBuilder.DropTable(
                name: "OutboxMessages");
        }
    }
}