using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class workoutStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
               name: "UserWorkoutStats",
               columns: table => new
               {
                   UserWorkoutStatsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                   UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                   WorkoutCount = table.Column<int>(type: "int", nullable: false),
                   TotalWorkoutDuration = table.Column<int>(type: "int", nullable: false),
                   CardioCompletedCount = table.Column<int>(type: "int", nullable: false),
                   HIITCompletedCount = table.Column<int>(type: "int", nullable: false),
                   StrengthTrainingCompletedCount = table.Column<int>(type: "int", nullable: false),
                   RunningCompletedCount = table.Column<int>(type: "int", nullable: false),
                   YogaCompletedCount = table.Column<int>(type: "int", nullable: false),
                   PilatesCompletedCount = table.Column<int>(type: "int", nullable: false),
                   BalancedWorkoutsCompletedCount = table.Column<int>(type: "int", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_UserWorkoutStats", x => x.UserWorkoutStatsId);
                   table.ForeignKey(
                       name: "FK_UserWorkoutStats_AspNetUsers_UserId",
                       column: x => x.UserId,
                       principalTable: "AspNetUsers",
                       principalColumn: "Id",
                       onDelete: ReferentialAction.Cascade);
               });

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkoutStats_UserId",
                table: "UserWorkoutStats",
                column: "UserId");
        }
    }
}
