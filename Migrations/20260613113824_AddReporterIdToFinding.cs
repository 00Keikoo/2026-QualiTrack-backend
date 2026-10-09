using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddReporterIdToFinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReporterId",
                table: "Findings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Findings_ReporterId",
                table: "Findings",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Findings_Users_ReporterId",
                table: "Findings",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Findings_Users_ReporterId",
                table: "Findings");

            migrationBuilder.DropIndex(
                name: "IX_Findings_ReporterId",
                table: "Findings");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "Findings");
        }
    }
}
