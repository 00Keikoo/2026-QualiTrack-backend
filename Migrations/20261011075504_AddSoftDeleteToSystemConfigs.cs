using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToSystemConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteAt",
                table: "FindingCategories",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "FindingCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteAt",
                table: "CapaStatuses",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedById",
                table: "CapaStatuses",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteAt",
                table: "FindingCategories");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "FindingCategories");

            migrationBuilder.DropColumn(
                name: "DeleteAt",
                table: "CapaStatuses");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "CapaStatuses");
        }
    }
}
