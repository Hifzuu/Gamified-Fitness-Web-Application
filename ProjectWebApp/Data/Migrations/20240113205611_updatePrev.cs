using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class updatePrev : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteWorkouts_AspNetUsers_UserId",
                table: "UserFavoriteWorkouts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteWorkouts_Workouts_WorkoutId",
                table: "UserFavoriteWorkouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavoriteWorkouts",
                table: "UserFavoriteWorkouts");

            migrationBuilder.RenameTable(
                name: "UserFavoriteWorkouts",
                newName: "UserFavouriteWorkouts");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavoriteWorkouts_WorkoutId",
                table: "UserFavouriteWorkouts",
                newName: "IX_UserFavouriteWorkouts_WorkoutId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavouriteWorkouts",
                table: "UserFavouriteWorkouts",
                columns: new[] { "UserId", "WorkoutId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouriteWorkouts_AspNetUsers_UserId",
                table: "UserFavouriteWorkouts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavouriteWorkouts_Workouts_WorkoutId",
                table: "UserFavouriteWorkouts",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "WorkoutId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouriteWorkouts_AspNetUsers_UserId",
                table: "UserFavouriteWorkouts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavouriteWorkouts_Workouts_WorkoutId",
                table: "UserFavouriteWorkouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavouriteWorkouts",
                table: "UserFavouriteWorkouts");

            migrationBuilder.RenameTable(
                name: "UserFavouriteWorkouts",
                newName: "UserFavoriteWorkouts");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavouriteWorkouts_WorkoutId",
                table: "UserFavoriteWorkouts",
                newName: "IX_UserFavoriteWorkouts_WorkoutId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavoriteWorkouts",
                table: "UserFavoriteWorkouts",
                columns: new[] { "UserId", "WorkoutId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteWorkouts_AspNetUsers_UserId",
                table: "UserFavoriteWorkouts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteWorkouts_Workouts_WorkoutId",
                table: "UserFavoriteWorkouts",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "WorkoutId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
