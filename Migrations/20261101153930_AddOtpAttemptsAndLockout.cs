using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpAttemptsAndLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtpAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpLockoutEnd",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OtpLockoutEnd",
                table: "Users");
        }
    }
}
