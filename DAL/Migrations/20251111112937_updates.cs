using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class updates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Cashboxes_CashboxId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cashboxes",
                table: "Cashboxes");

            migrationBuilder.RenameTable(
                name: "Cashboxes",
                newName: "CashBoxes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CashBoxes",
                table: "CashBoxes",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "64f827b4-3c40-408c-8977-4b236c13dd68", new DateTime(2025, 11, 11, 11, 29, 37, 171, DateTimeKind.Utc).AddTicks(7530), "AQAAAAIAAYagAAAAEFcoDbPAnmYZbGoxhAJUp4C3MYmEdspikKBj33DBi5A0LevQOFa1qRpOoXco10r0DQ==", "3a7b337b-047b-4e5b-80d3-fd7ee2665d39" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CashBoxes_CashboxId",
                table: "Transactions",
                column: "CashboxId",
                principalTable: "CashBoxes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CashBoxes_CashboxId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CashBoxes",
                table: "CashBoxes");

            migrationBuilder.RenameTable(
                name: "CashBoxes",
                newName: "Cashboxes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cashboxes",
                table: "Cashboxes",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "90a54122-810a-4716-900e-685f87ee1058", new DateTime(2025, 11, 9, 12, 2, 24, 48, DateTimeKind.Utc).AddTicks(878), "AQAAAAIAAYagAAAAEGMNG/Ym8qNFO4pGu4elC5ZYu/l5Elfm7arS5XqqsRAm2TN1ZHjGB8Oi0SD/o173jg==", "d65eb076-d7b7-4ee1-bfcb-014cef555697" });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Cashboxes_CashboxId",
                table: "Transactions",
                column: "CashboxId",
                principalTable: "Cashboxes",
                principalColumn: "Id");
        }
    }
}
