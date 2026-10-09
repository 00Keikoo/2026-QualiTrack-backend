using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddSpcAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpcAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Lsl = table.Column<double>(type: "double precision", nullable: false),
                    Usl = table.Column<double>(type: "double precision", nullable: false),
                    Mean = table.Column<double>(type: "double precision", nullable: false),
                    StandardDeviation = table.Column<double>(type: "double precision", nullable: false),
                    Ucl = table.Column<double>(type: "double precision", nullable: false),
                    Lcl = table.Column<double>(type: "double precision", nullable: false),
                    Cp = table.Column<double>(type: "double precision", nullable: false),
                    Cpk = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataCount = table.Column<int>(type: "integer", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AnalyzedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpcAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpcAnalyses_Users_AnalyzedById",
                        column: x => x.AnalyzedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpcAnalyses_AnalyzedById",
                table: "SpcAnalyses",
                column: "AnalyzedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpcAnalyses");
        }
    }
}
