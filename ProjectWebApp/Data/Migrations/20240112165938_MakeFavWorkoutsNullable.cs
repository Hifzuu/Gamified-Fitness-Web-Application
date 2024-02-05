using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeFavWorkoutsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserFavouriteWorkouts",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Workouts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_ApplicationUserId",
                table: "Workouts",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_AspNetUsers_ApplicationUserId",
                table: "Workouts",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_AspNetUsers_ApplicationUserId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_ApplicationUserId",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Workouts");

            migrationBuilder.AddColumn<string>(
                name: "UserFavouriteWorkouts",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
