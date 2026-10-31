using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceFilePolymorphicConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FindingId1",
                table: "EvidenceFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceFiles_FindingId1",
                table: "EvidenceFiles",
                column: "FindingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId1",
                table: "EvidenceFiles",
                column: "FindingId1",
                principalTable: "Findings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId1",
                table: "EvidenceFiles");

            migrationBuilder.DropIndex(
                name: "IX_EvidenceFiles_FindingId1",
                table: "EvidenceFiles");

            migrationBuilder.DropColumn(
                name: "FindingId1",
                table: "EvidenceFiles");
        }
    }
}
