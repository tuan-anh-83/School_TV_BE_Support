using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdLiveStreamAccountID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountID",
                table: "AdLiveStream",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AdLiveStream_AccountID",
                table: "AdLiveStream",
                column: "AccountID");

            migrationBuilder.AddForeignKey(
                name: "FK_AdLiveStream_Account_AccountID",
                table: "AdLiveStream",
                column: "AccountID",
                principalTable: "Account",
                principalColumn: "AccountID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdLiveStream_Account_AccountID",
                table: "AdLiveStream");

            migrationBuilder.DropIndex(
                name: "IX_AdLiveStream_AccountID",
                table: "AdLiveStream");

            migrationBuilder.DropColumn(
                name: "AccountID",
                table: "AdLiveStream");
        }
    }
}
