using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSalon.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "RatingAvg",
                table: "Artists",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Artists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRated",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsRated",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Appointments");

            migrationBuilder.AlterColumn<double>(
                name: "RatingAvg",
                table: "Artists",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
