using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddHandoverreturnandManagerReturnRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HandoverReturn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HandoverTicketId = table.Column<int>(type: "int", nullable: false),
                    ReturnById = table.Column<int>(type: "int", nullable: true),
                    ReceivedById = table.Column<int>(type: "int", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetConditionOnReturn = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoverReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoverReturn_AspNetUsers_ReceivedById",
                        column: x => x.ReceivedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HandoverReturn_AspNetUsers_ReturnById",
                        column: x => x.ReturnById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HandoverReturn_HandoverTickets_HandoverTicketId",
                        column: x => x.HandoverTicketId,
                        principalTable: "HandoverTickets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ManagerReturnRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BorrowTicketId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedReturnTicketId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerReturnRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagerReturnRequests_AspNetUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManagerReturnRequests_BorrowTickets_BorrowTicketId",
                        column: x => x.BorrowTicketId,
                        principalTable: "BorrowTickets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManagerReturnRequests_ReturnTickets_RelatedReturnTicketId",
                        column: x => x.RelatedReturnTicketId,
                        principalTable: "ReturnTickets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HandoverReturn_HandoverTicketId",
                table: "HandoverReturn",
                column: "HandoverTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoverReturn_ReceivedById",
                table: "HandoverReturn",
                column: "ReceivedById");

            migrationBuilder.CreateIndex(
                name: "IX_HandoverReturn_ReturnById",
                table: "HandoverReturn",
                column: "ReturnById");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerReturnRequests_BorrowTicketId",
                table: "ManagerReturnRequests",
                column: "BorrowTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerReturnRequests_RelatedReturnTicketId",
                table: "ManagerReturnRequests",
                column: "RelatedReturnTicketId",
                unique: true,
                filter: "[RelatedReturnTicketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerReturnRequests_RequestedById",
                table: "ManagerReturnRequests",
                column: "RequestedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HandoverReturn");

            migrationBuilder.DropTable(
                name: "ManagerReturnRequests");
        }
    }
}
