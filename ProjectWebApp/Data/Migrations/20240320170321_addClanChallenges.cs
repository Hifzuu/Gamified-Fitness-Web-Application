using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addClanChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClanChallengeId",
                table: "Workouts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClanChallenges",
                columns: table => new
                {
                    ClanChallengeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClanId = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CountProgress = table.Column<int>(type: "int", nullable: false),
                    IsRewardClaimed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanChallenges", x => x.ClanChallengeId);
                    table.ForeignKey(
                        name: "FK_ClanChallenges_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanChallenges_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "ClanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_ClanChallengeId",
                table: "Workouts",
                column: "ClanChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanChallenges_ChallengeId",
                table: "ClanChallenges",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanChallenges_ClanId",
                table: "ClanChallenges",
                column: "ClanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_ClanChallenges_ClanChallengeId",
                table: "Workouts",
                column: "ClanChallengeId",
                principalTable: "ClanChallenges",
                principalColumn: "ClanChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_ClanChallenges_ClanChallengeId",
                table: "Workouts");

            migrationBuilder.DropTable(
                name: "ClanChallenges");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_ClanChallengeId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "ClanChallengeId",
                table: "Workouts");
        }
    }
}
