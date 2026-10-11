using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOrderIndexConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Findings",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusId",
                table: "CAPAs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "IX_CapaStatuses_OrderIndex",
                table: "CapaStatuses",
                column: "OrderIndex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FindingCategories_Severity",
                table: "FindingCategories",
                column: "Severity",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CapaStatuses_OrderIndex",
                table: "CapaStatuses");

            migrationBuilder.DropIndex(
                name: "IX_FindingCategories_Severity",
                table: "FindingCategories");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Findings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StatusId",
                table: "CAPAs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
