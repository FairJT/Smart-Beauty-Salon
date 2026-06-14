using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSalon.Migrations
{
    /// <inheritdoc />
    public partial class DashboardEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalonServices_SalonId",
                table: "SalonServices");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Artists_SalonId",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ArtistId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SalonId",
                table: "Appointments");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SalonPackageSubscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SalonServices_SalonId_IsActive",
                table: "SalonServices",
                columns: new[] { "SalonId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Salons_IsActive",
                table: "Salons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SalonId_IsActive",
                table: "Artists",
                columns: new[] { "SalonId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ArtistId_StartTime",
                table: "Appointments",
                columns: new[] { "ArtistId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClientId_Status",
                table: "Appointments",
                columns: new[] { "ClientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SalonId_Status",
                table: "Appointments",
                columns: new[] { "SalonId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalonServices_SalonId_IsActive",
                table: "SalonServices");

            migrationBuilder.DropIndex(
                name: "IX_Salons_IsActive",
                table: "Salons");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Artists_SalonId_IsActive",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ArtistId_StartTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClientId_Status",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_SalonId_Status",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SalonPackageSubscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_SalonServices_SalonId",
                table: "SalonServices",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SalonId",
                table: "Artists",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ArtistId",
                table: "Appointments",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClientId",
                table: "Appointments",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SalonId",
                table: "Appointments",
                column: "SalonId");
        }
    }
}
