using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfigTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CAPAs_Status_Deadline",
                table: "CAPAs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Findings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CAPAs");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Findings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "CAPAs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CapaStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapaStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FindingCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RequiresImmediateAction = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FindingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Findings_CategoryId",
                table: "Findings",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CAPAs_StatusId",
                table: "CAPAs",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CAPAs_StatusId_Deadline",
                table: "CAPAs",
                columns: new[] { "StatusId", "Deadline" });

            migrationBuilder.CreateIndex(
                name: "IX_CapaStatuses_Name",
                table: "CapaStatuses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FindingCategories_Name",
                table: "FindingCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigs_Key",
                table: "SystemConfigs",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CAPAs_CapaStatuses_StatusId",
                table: "CAPAs",
                column: "StatusId",
                principalTable: "CapaStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Findings_FindingCategories_CategoryId",
                table: "Findings",
                column: "CategoryId",
                principalTable: "FindingCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPAs_CapaStatuses_StatusId",
                table: "CAPAs");

            migrationBuilder.DropForeignKey(
                name: "FK_Findings_FindingCategories_CategoryId",
                table: "Findings");

            migrationBuilder.DropTable(
                name: "CapaStatuses");

            migrationBuilder.DropTable(
                name: "FindingCategories");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropIndex(
                name: "IX_Findings_CategoryId",
                table: "Findings");

            migrationBuilder.DropIndex(
                name: "IX_CAPAs_StatusId",
                table: "CAPAs");

            migrationBuilder.DropIndex(
                name: "IX_CAPAs_StatusId_Deadline",
                table: "CAPAs");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Findings");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "CAPAs");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Findings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CAPAs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CAPAs_Status_Deadline",
                table: "CAPAs",
                columns: new[] { "Status", "Deadline" });
        }
    }
}
