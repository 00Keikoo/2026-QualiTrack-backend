using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceFileRelationsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_AuditResponses_AuditResponseId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CapaActionId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId1",
                table: "EvidenceFiles");

            migrationBuilder.RenameColumn(
                name: "FindingId1",
                table: "EvidenceFiles",
                newName: "CAPAActionId");

            migrationBuilder.RenameIndex(
                name: "IX_EvidenceFiles_FindingId1",
                table: "EvidenceFiles",
                newName: "IX_EvidenceFiles_CAPAActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_AuditResponses_AuditResponseId",
                table: "EvidenceFiles",
                column: "AuditResponseId",
                principalTable: "AuditResponses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CAPAActionId",
                table: "EvidenceFiles",
                column: "CAPAActionId",
                principalTable: "CAPAActions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CapaActionId",
                table: "EvidenceFiles",
                column: "CapaActionId",
                principalTable: "CAPAActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles",
                column: "FindingId",
                principalTable: "Findings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_AuditResponses_AuditResponseId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CAPAActionId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CapaActionId",
                table: "EvidenceFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles");

            migrationBuilder.RenameColumn(
                name: "CAPAActionId",
                table: "EvidenceFiles",
                newName: "FindingId1");

            migrationBuilder.RenameIndex(
                name: "IX_EvidenceFiles_CAPAActionId",
                table: "EvidenceFiles",
                newName: "IX_EvidenceFiles_FindingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_AuditResponses_AuditResponseId",
                table: "EvidenceFiles",
                column: "AuditResponseId",
                principalTable: "AuditResponses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_CAPAActions_CapaActionId",
                table: "EvidenceFiles",
                column: "CapaActionId",
                principalTable: "CAPAActions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId",
                table: "EvidenceFiles",
                column: "FindingId",
                principalTable: "Findings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceFiles_Findings_FindingId1",
                table: "EvidenceFiles",
                column: "FindingId1",
                principalTable: "Findings",
                principalColumn: "Id");
        }
    }
}
