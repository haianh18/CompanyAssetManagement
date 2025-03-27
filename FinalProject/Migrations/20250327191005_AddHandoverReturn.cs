using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddHandoverReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetStatus",
                table: "Assets");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "WarehouseAssets",
                newName: "HandedOverGoodQuantity");

            migrationBuilder.AddColumn<int>(
                name: "BorrowedGoodQuantity",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BrokenQuantity",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisposedQuantity",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FixingQuantity",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GoodQuantity",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualReturnDate",
                table: "ReturnTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetConditionOnReturn",
                table: "ReturnTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEarlyReturn",
                table: "ReturnTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnRequestDate",
                table: "ReturnTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "HandoverTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentCondition",
                table: "HandoverTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedEndDate",
                table: "HandoverTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "HandoverTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BorrowedAssetStatus",
                table: "BorrowTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionApproveStatus",
                table: "BorrowTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExtensionBorrowTicketId",
                table: "BorrowTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExtensionRequestDate",
                table: "BorrowTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtended",
                table: "BorrowTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "BorrowTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowTickets_ExtensionBorrowTicketId",
                table: "BorrowTickets",
                column: "ExtensionBorrowTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowTickets_BorrowTickets_ExtensionBorrowTicketId",
                table: "BorrowTickets",
                column: "ExtensionBorrowTicketId",
                principalTable: "BorrowTickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BorrowTickets_BorrowTickets_ExtensionBorrowTicketId",
                table: "BorrowTickets");

            migrationBuilder.DropIndex(
                name: "IX_BorrowTickets_ExtensionBorrowTicketId",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "BorrowedGoodQuantity",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "BrokenQuantity",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "DisposedQuantity",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "FixingQuantity",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "GoodQuantity",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "ActualReturnDate",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "AssetConditionOnReturn",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "IsEarlyReturn",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "ReturnRequestDate",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "CurrentCondition",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "ExpectedEndDate",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "BorrowedAssetStatus",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "ExtensionApproveStatus",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "ExtensionBorrowTicketId",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "ExtensionRequestDate",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "IsExtended",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "BorrowTickets");

            migrationBuilder.RenameColumn(
                name: "HandedOverGoodQuantity",
                table: "WarehouseAssets",
                newName: "Quantity");

            migrationBuilder.AddColumn<int>(
                name: "AssetStatus",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
