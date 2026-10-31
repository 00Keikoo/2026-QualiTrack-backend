using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class FixEvidenceFilePolymorphicConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hapus kolom duplikat CAPAActionId jika ada
            migrationBuilder.DropColumn(
                name: "CAPAActionId",
                table: "EvidenceFiles"
            );

            // Tambah CHECK constraint untuk polymorphic exclusivity
            migrationBuilder.Sql(@"
              ALTER TABLE ""EvidenceFiles"" 
              ADD CONSTRAINT CHK_EvidenceFile_SingleForeignKey 
              CHECK (
                  (""FindingId"" IS NOT NULL AND ""AuditResponseId"" IS NULL AND ""CapaActionId"" IS NULL) OR
                  (""FindingId"" IS NULL AND ""AuditResponseId"" IS NOT NULL AND ""CapaActionId"" IS NULL) OR
                  (""FindingId"" IS NULL AND ""AuditResponseId"" IS NULL AND ""CapaActionId"" IS NOT NULL)
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
              ALTER TABLE ""EvidenceFiles""
              DROP CONSTRAINT IF EXISTS CHK_EvidenceFile_SingleForeignKey
          ");
        }
    }
}
