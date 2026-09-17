using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddCapaActionAndCloseOutNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CloseOutVerifications_VerifiedById",
                table: "CloseOutVerifications",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_CAPAActions_DoneById",
                table: "CAPAActions",
                column: "DoneById");

            migrationBuilder.AddForeignKey(
                name: "FK_CAPAActions_Users_DoneById",
                table: "CAPAActions",
                column: "DoneById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CloseOutVerifications_Users_VerifiedById",
                table: "CloseOutVerifications",
                column: "VerifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CAPAActions_Users_DoneById",
                table: "CAPAActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CloseOutVerifications_Users_VerifiedById",
                table: "CloseOutVerifications");

            migrationBuilder.DropIndex(
                name: "IX_CloseOutVerifications_VerifiedById",
                table: "CloseOutVerifications");

            migrationBuilder.DropIndex(
                name: "IX_CAPAActions_DoneById",
                table: "CAPAActions");
        }
    }
}
