using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Employees",
                newName: "PhoneNumber");

            migrationBuilder.AlterColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "90a54122-810a-4716-900e-685f87ee1058", new DateTime(2025, 11, 9, 12, 2, 24, 48, DateTimeKind.Utc).AddTicks(878), "AQAAAAIAAYagAAAAEGMNG/Ym8qNFO4pGu4elC5ZYu/l5Elfm7arS5XqqsRAm2TN1ZHjGB8Oi0SD/o173jg==", "d65eb076-d7b7-4ee1-bfcb-014cef555697" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Employees",
                newName: "Phone");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "Stock",
                table: "Products",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bc57d436-8316-43e8-b856-355da8704992", new DateTime(2025, 11, 9, 10, 40, 21, 150, DateTimeKind.Local).AddTicks(1124), "AQAAAAIAAYagAAAAEEcL1JwedYxocGIY+CjddAu8MO1zP99wicGqCoAiOauakuytPqj0wMf/MfcOHEmHGw==", "" });
        }
    }
}
