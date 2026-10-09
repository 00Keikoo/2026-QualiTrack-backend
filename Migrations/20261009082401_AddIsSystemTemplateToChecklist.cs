using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemTemplateToChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemTemplate",
                table: "Checklists",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystemTemplate",
                table: "Checklists");
        }
    }
}
