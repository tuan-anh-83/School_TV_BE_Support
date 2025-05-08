using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountPackageToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalHoursAllowed",
                table: "AccountPackage",
                newName: "TotalMinutesAllowed");

            migrationBuilder.RenameColumn(
                name: "RemainingHours",
                table: "AccountPackage",
                newName: "RemainingMinutes");

            migrationBuilder.RenameColumn(
                name: "HoursUsed",
                table: "AccountPackage",
                newName: "MinutesUsed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalMinutesAllowed",
                table: "AccountPackage",
                newName: "TotalHoursAllowed");

            migrationBuilder.RenameColumn(
                name: "RemainingMinutes",
                table: "AccountPackage",
                newName: "RemainingHours");

            migrationBuilder.RenameColumn(
                name: "MinutesUsed",
                table: "AccountPackage",
                newName: "HoursUsed");
        }
    }
}
