using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addParticipantListClan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClanChallengeId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClanChallengeId",
                table: "AspNetUsers",
                column: "ClanChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers",
                column: "ClanChallengeId",
                principalTable: "ClanChallenges",
                principalColumn: "ClanChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ClanChallenges_ClanChallengeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClanChallengeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClanChallengeId",
                table: "AspNetUsers");
        }
    }
}
