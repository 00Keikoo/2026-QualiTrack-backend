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

            // 1. Tambah kolom sebagai NULLABLE dulu (tanpa default)
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Findings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                table: "CAPAs",
                type: "uuid",
                nullable: true);

            // 2. Buat tabel master
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

            // 3. Seed data master default (WAJIB — agar FK bisa dipenuhi)
            migrationBuilder.InsertData(
                table: "FindingCategories",
                columns: new[] { "Id", "Name", "Description", "Severity", "RequiresImmediateAction", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Minor NC",    "Minor non-conformance",  1, false, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Major NC",    "Major non-conformance",  2, true,  true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Observation", "Observation / OFI",      0, false, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                });

            migrationBuilder.InsertData(
                table: "CapaStatuses",
                columns: new[] { "Id", "Name", "Description", "OrderIndex", "IsTerminal", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Open",        "CAPA baru dibuat",       1, false, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "In Progress", "CAPA sedang dikerjakan", 2, false, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Closed",      "CAPA sudah ditutup",     3, true,  true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                });

            // 4. Update baris lama (jika ada) ke default — sebelum NOT NULL
            migrationBuilder.Sql(@"
                UPDATE ""Findings"" SET ""CategoryId"" = '11111111-1111-1111-1111-111111111111'
                WHERE ""CategoryId"" IS NULL;

                UPDATE ""CAPAs"" SET ""StatusId"" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
                WHERE ""StatusId"" IS NULL;
            ");

            // 5. Baru jadikan NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Findings",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusId",
                table: "CAPAs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 6. Index (tanpa unique untuk CapaStatuses.Name / FindingCategories.Name —
            //    karena soft delete belum ada di tabel ini; tambahkan nanti jika perlu)
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

            // 7. FK constraint — sekarang aman karena seed sudah ada
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

            migrationBuilder.DropTable(name: "CapaStatuses");
            migrationBuilder.DropTable(name: "FindingCategories");
            migrationBuilder.DropTable(name: "SystemConfigs");

            migrationBuilder.DropIndex(name: "IX_Findings_CategoryId", table: "Findings");
            migrationBuilder.DropIndex(name: "IX_CAPAs_StatusId", table: "CAPAs");
            migrationBuilder.DropIndex(name: "IX_CAPAs_StatusId_Deadline", table: "CAPAs");

            migrationBuilder.DropColumn(name: "CategoryId", table: "Findings");
            migrationBuilder.DropColumn(name: "StatusId", table: "CAPAs");

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