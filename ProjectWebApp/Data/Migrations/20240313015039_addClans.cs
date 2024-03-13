using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class addClans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClanId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    ClanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.ClanId);
                    table.ForeignKey(
                        name: "FK_Clans_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClanId",
                table: "AspNetUsers",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_CreatorId",
                table: "Clans",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Clans_ClanId",
                table: "AspNetUsers",
                column: "ClanId",
                principalTable: "Clans",
                principalColumn: "ClanId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Clans_ClanId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClanId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClanId",
                table: "AspNetUsers");
        }
    }
}
