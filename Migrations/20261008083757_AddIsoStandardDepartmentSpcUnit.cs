using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddIsoStandardDepartmentSpcUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Checklists",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IsoStandardId",
                table: "Checklists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IsoStandards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsoStandards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpcUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpcUnits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_DepartmentId",
                table: "Checklists",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_IsoStandardId",
                table: "Checklists",
                column: "IsoStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_IsoStandards_Code",
                table: "IsoStandards",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpcUnits_Name",
                table: "SpcUnits",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_Departments_DepartmentId",
                table: "Checklists",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Checklists_IsoStandards_IsoStandardId",
                table: "Checklists",
                column: "IsoStandardId",
                principalTable: "IsoStandards",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_Departments_DepartmentId",
                table: "Checklists");

            migrationBuilder.DropForeignKey(
                name: "FK_Checklists_IsoStandards_IsoStandardId",
                table: "Checklists");

            migrationBuilder.DropTable(
                name: "IsoStandards");

            migrationBuilder.DropTable(
                name: "SpcUnits");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_DepartmentId",
                table: "Checklists");

            migrationBuilder.DropIndex(
                name: "IX_Checklists_IsoStandardId",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Checklists");

            migrationBuilder.DropColumn(
                name: "IsoStandardId",
                table: "Checklists");
        }
    }
}
