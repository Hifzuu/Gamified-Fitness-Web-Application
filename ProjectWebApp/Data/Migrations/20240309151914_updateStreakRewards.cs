using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateStreakRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreakRewards_LoginStreaks_LoginStreakUserId_LoginStreakLastLoginTime",
                table: "StreakRewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserStreakRewards",
                table: "UserStreakRewards");

            migrationBuilder.DropIndex(
                name: "IX_StreakRewards_LoginStreakUserId_LoginStreakLastLoginTime",
                table: "StreakRewards");

            migrationBuilder.DropColumn(
                name: "Claimed",
                table: "StreakRewards");

            migrationBuilder.DropColumn(
                name: "LoginStreakLastLoginTime",
                table: "StreakRewards");

            migrationBuilder.DropColumn(
                name: "LoginStreakUserId",
                table: "StreakRewards");

            migrationBuilder.AddColumn<int>(
                name: "UserStreakRewardId",
                table: "UserStreakRewards",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "Claimed",
                table: "UserStreakRewards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserStreakRewards",
                table: "UserStreakRewards",
                column: "UserStreakRewardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStreakRewards_UserId_LastLoginTime",
                table: "UserStreakRewards",
                columns: new[] { "UserId", "LastLoginTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserStreakRewards",
                table: "UserStreakRewards");

            migrationBuilder.DropIndex(
                name: "IX_UserStreakRewards_UserId_LastLoginTime",
                table: "UserStreakRewards");

            migrationBuilder.DropColumn(
                name: "UserStreakRewardId",
                table: "UserStreakRewards");

            migrationBuilder.DropColumn(
                name: "Claimed",
                table: "UserStreakRewards");

            migrationBuilder.AddColumn<bool>(
                name: "Claimed",
                table: "StreakRewards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LoginStreakLastLoginTime",
                table: "StreakRewards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginStreakUserId",
                table: "StreakRewards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserStreakRewards",
                table: "UserStreakRewards",
                columns: new[] { "UserId", "LastLoginTime", "RewardId" });

            migrationBuilder.CreateIndex(
                name: "IX_StreakRewards_LoginStreakUserId_LoginStreakLastLoginTime",
                table: "StreakRewards",
                columns: new[] { "LoginStreakUserId", "LoginStreakLastLoginTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_StreakRewards_LoginStreaks_LoginStreakUserId_LoginStreakLastLoginTime",
                table: "StreakRewards",
                columns: new[] { "LoginStreakUserId", "LoginStreakLastLoginTime" },
                principalTable: "LoginStreaks",
                principalColumns: new[] { "UserId", "LastLoginTime" });
        }
    }
}
