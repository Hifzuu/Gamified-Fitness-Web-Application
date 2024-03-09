using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StreakRewards",
                columns: table => new
                {
                    RewardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedalText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Days = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Claimed = table.Column<bool>(type: "bit", nullable: false),
                    LoginStreakLastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoginStreakUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreakRewards", x => x.RewardId);
                    table.ForeignKey(
                        name: "FK_StreakRewards_LoginStreaks_LoginStreakUserId_LoginStreakLastLoginTime",
                        columns: x => new { x.LoginStreakUserId, x.LoginStreakLastLoginTime },
                        principalTable: "LoginStreaks",
                        principalColumns: new[] { "UserId", "LastLoginTime" });
                });

            migrationBuilder.CreateTable(
                name: "UserStreakRewards",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RewardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStreakRewards", x => new { x.UserId, x.LastLoginTime, x.RewardId });
                    table.ForeignKey(
                        name: "FK_UserStreakRewards_LoginStreaks_UserId_LastLoginTime",
                        columns: x => new { x.UserId, x.LastLoginTime },
                        principalTable: "LoginStreaks",
                        principalColumns: new[] { "UserId", "LastLoginTime" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStreakRewards_StreakRewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "StreakRewards",
                        principalColumn: "RewardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreakRewards_LoginStreakUserId_LoginStreakLastLoginTime",
                table: "StreakRewards",
                columns: new[] { "LoginStreakUserId", "LoginStreakLastLoginTime" });

            migrationBuilder.CreateIndex(
                name: "IX_UserStreakRewards_RewardId",
                table: "UserStreakRewards",
                column: "RewardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStreakRewards");

            migrationBuilder.DropTable(
                name: "StreakRewards");
        }
    }
}
