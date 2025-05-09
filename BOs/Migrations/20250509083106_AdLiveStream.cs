using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BOs.Migrations
{
    /// <inheritdoc />
    public partial class AdLiveStream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdLiveStream",
                columns: table => new
                {
                    AdLiveStreamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdScheduleID = table.Column<int>(type: "int", nullable: false),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    PlayAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPlayed = table.Column<bool>(type: "bit", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdLiveStream", x => x.AdLiveStreamID);
                    table.ForeignKey(
                        name: "FK_AdLiveStream_AdSchedule_AdScheduleID",
                        column: x => x.AdScheduleID,
                        principalTable: "AdSchedule",
                        principalColumn: "AdScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdLiveStream_Schedule_ScheduleID",
                        column: x => x.ScheduleID,
                        principalTable: "Schedule",
                        principalColumn: "ScheduleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdLiveStream_AdScheduleID",
                table: "AdLiveStream",
                column: "AdScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_AdLiveStream_ScheduleID",
                table: "AdLiveStream",
                column: "ScheduleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdLiveStream");
        }
    }
}
