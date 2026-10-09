using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingTemplatesAsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Checklists""
                SET ""IsSystemTemplate"" = true
                WHERE ""Title"" IN (
                    'ISO 9001 - Warehouse',
                    'ISO 14001 - Warehouse',
                    'GMP - Warehouse',
                    'ISO 9001 - Production',
                    'ISO 14001 - Production',
                    'GMP - Production',
                    'ISO 9001 - QC',
                    'ISO 14001 - QC',
                    'GMP - QC',
                    'ISO 9001 - Packaging',
                    'ISO 14001 - Packaging',
                    'GMP - Packaging'
            )");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}


