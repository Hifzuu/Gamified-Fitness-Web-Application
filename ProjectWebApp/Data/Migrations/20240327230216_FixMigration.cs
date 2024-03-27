using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ClanChallengeId1",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClanChallengeId1",
                table: "AspNetUsers",
                column: "ClanChallengeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers",
                column: "ClanChallengeId",
                principalTable: "ClanChallenges",
                principalColumn: "ClanChallengeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId1",
                table: "AspNetUsers",
                column: "ClanChallengeId1",
                principalTable: "ClanChallenges",
                principalColumn: "ClanChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId1",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClanChallengeId1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClanChallengeId1",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers",
                column: "ClanChallengeId",
                principalTable: "ClanChallenges",
                principalColumn: "ClanChallengeId");
        }
    }
}
