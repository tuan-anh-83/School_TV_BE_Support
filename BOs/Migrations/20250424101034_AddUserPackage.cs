using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOs.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountPackage",
                columns: table => new
                {
                    AccountPackageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountID = table.Column<int>(type: "int", nullable: false),
                    PackageID = table.Column<int>(type: "int", nullable: false),
                    TotalHoursAllowed = table.Column<double>(type: "float", nullable: false),
                    HoursUsed = table.Column<double>(type: "float", nullable: false),
                    RemainingHours = table.Column<double>(type: "float", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()"),
                    AccountID1 = table.Column<int>(type: "int", nullable: true),
                    PackageID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountPackage", x => x.AccountPackageID);
                    table.ForeignKey(
                        name: "FK_AccountPackage_Account_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Account",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountPackage_Account_AccountID1",
                        column: x => x.AccountID1,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK_AccountPackage_Package_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Package",
                        principalColumn: "PackageID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountPackage_Package_PackageID1",
                        column: x => x.PackageID1,
                        principalTable: "Package",
                        principalColumn: "PackageID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountPackage_AccountID",
                table: "AccountPackage",
                column: "AccountID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPackage_AccountID1",
                table: "AccountPackage",
                column: "AccountID1");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPackage_PackageID",
                table: "AccountPackage",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_AccountPackage_PackageID1",
                table: "AccountPackage",
                column: "PackageID1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountPackage");
        }
    }
}
