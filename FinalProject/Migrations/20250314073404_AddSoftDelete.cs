using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Warehouses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "WarehouseAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "WarehouseAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "WarehouseAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WarehouseAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "ReturnTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ReturnTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ReturnTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "HandoverTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "HandoverTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HandoverTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "DisposalTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "DisposalTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DisposalTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "DisposalTicketAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "DisposalTicketAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "DisposalTicketAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "DisposalTicketAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DisposalTicketAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "BorrowTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "BorrowTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BorrowTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Assets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Assets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "AssetCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AssetCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "DisposalTickets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "DisposalTickets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DisposalTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "AssetCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AssetCategories");
        }
    }
}
