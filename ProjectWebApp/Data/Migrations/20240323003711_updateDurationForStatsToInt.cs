using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateDurationForStatsToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop default constraint
            migrationBuilder.Sql("ALTER TABLE UserWorkoutStats DROP CONSTRAINT <constraint_name>");

            // Alter column type
            migrationBuilder.AlterColumn<int>(
                name: "TotalWorkoutDuration",
                table: "UserWorkoutStats",
                type: "int",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TotalWorkoutDuration",
                table: "UserWorkoutStats",
                type: "time",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
