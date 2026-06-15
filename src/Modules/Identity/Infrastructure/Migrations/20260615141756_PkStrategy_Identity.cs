using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonOS.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PkStrategy_Identity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedSalons",
                table: "SavedSalons");

            migrationBuilder.DropIndex(
                name: "IX_SavedSalons_UserId",
                table: "SavedSalons");

            migrationBuilder.DropIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_UserId_TenantId",
                table: "Memberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedSalons",
                table: "SavedSalons",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons",
                columns: new[] { "UserId", "Slug" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TenantId_UserId",
                table: "Memberships",
                columns: new[] { "TenantId", "UserId" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SavedSalons",
                table: "SavedSalons");

            migrationBuilder.DropIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_TenantId_UserId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_UserId",
                table: "Memberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SavedSalons",
                table: "SavedSalons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSalons_UserId",
                table: "SavedSalons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSalons_UserId_Slug",
                table: "SavedSalons",
                columns: new[] { "UserId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserId_TenantId",
                table: "Memberships",
                columns: new[] { "UserId", "TenantId" },
                unique: true);
        }
    }
}
