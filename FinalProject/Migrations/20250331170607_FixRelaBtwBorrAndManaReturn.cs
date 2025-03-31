using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class FixRelaBtwBorrAndManaReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagerReturnRequests_BorrowTicketId",
                table: "ManagerReturnRequests");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerReturnRequests_BorrowTicketId",
                table: "ManagerReturnRequests",
                column: "BorrowTicketId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagerReturnRequests_BorrowTicketId",
                table: "ManagerReturnRequests");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerReturnRequests_BorrowTicketId",
                table: "ManagerReturnRequests",
                column: "BorrowTicketId");
        }
    }
}
