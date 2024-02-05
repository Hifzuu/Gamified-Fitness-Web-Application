using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFavoriteWorkout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "UserFavoriteWorkouts",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkoutId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteWorkouts", x => new { x.UserId, x.WorkoutId });
                    table.ForeignKey(
                        name: "FK_UserFavoriteWorkouts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoriteWorkouts_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "WorkoutId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteWorkouts_WorkoutId",
                table: "UserFavoriteWorkouts",
                column: "WorkoutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteWorkouts");

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
    }
}
