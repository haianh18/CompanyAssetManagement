using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    /// <inheritdoc />
    public partial class FixSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "WarehouseAssets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "ReturnTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "HandoverTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "DisposalTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "DisposalTicketAssets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "BorrowTickets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "AssetCategories");

            migrationBuilder.DropColumn(
                name: "ActiveStatus",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "Warehouses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "WarehouseAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "ReturnTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "HandoverTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "DisposalTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "DisposalTicketAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "BorrowTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "AssetCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
