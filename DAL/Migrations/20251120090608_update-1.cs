using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Employees_EmployeeId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_EmployeeId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Payment");

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "InvoiceItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "LockoutEnabled", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9933418f-0bcd-479f-8d3b-1385729a0b66", new DateTime(2025, 11, 20, 9, 6, 7, 50, DateTimeKind.Utc).AddTicks(2681), true, "AQAAAAIAAYagAAAAEKxpX+e1EYm7WL/HNAbmCitpW9Tg9Jb+y0m1I8+3JdIDJDWgZFX/cf1XsLMQNBJb/A==", "f1ab1aa5-45b7-4c11-9d56-df00a3f10bc4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "InvoiceItem");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "LockoutEnabled", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f448fddf-f99a-4c50-a7c1-d71fd2e6f193", new DateTime(2025, 11, 15, 14, 47, 38, 541, DateTimeKind.Utc).AddTicks(7338), false, "AQAAAAIAAYagAAAAEBrsoeinEoipagPvSU/AUNcb9U219+7NA9BsNk94asznseGmmUFRdBMn0GbVqtwU1w==", "08535539-5f04-40ff-a4f5-bebef60e331f" });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_EmployeeId",
                table: "Payment",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Employees_EmployeeId",
                table: "Payment",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
