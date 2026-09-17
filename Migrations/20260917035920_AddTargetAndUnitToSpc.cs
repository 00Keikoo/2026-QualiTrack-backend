using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetAndUnitToSpc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "SpcAnalyses",
                newName: "ParameterName");

            migrationBuilder.AddColumn<double>(
                name: "Target",
                table: "SpcAnalyses",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "SpcAnalyses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Target",
                table: "SpcAnalyses");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "SpcAnalyses");

            migrationBuilder.RenameColumn(
                name: "ParameterName",
                table: "SpcAnalyses",
                newName: "ProductName");
        }
    }
}
