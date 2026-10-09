using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidencesToFinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EvidenceFiles_FindingId",
                table: "EvidenceFiles",
                column: "FindingId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles",
                column: "FindingId",
                principalTable: "Findings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles");

            migrationBuilder.DropIndex(
                name: "IX_EvidenceFiles_FindingId",
                table: "EvidenceFiles");
        }
    }
}
