using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    public partial class PaymentLedgerUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "Payment",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBefore",
                table: "Payment",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartyName",
                table: "Payment",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartyType",
                table: "Payment",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Expense");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Payment",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Payment",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "دفعة");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Payment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE p
                SET
                    p.PartyType = CASE
                        WHEN p.CustomerId IS NOT NULL THEN 'Customer'
                        WHEN p.SupplierId IS NOT NULL THEN 'Supplier'
                        ELSE 'Expense'
                    END,
                    p.PartyName = COALESCE(c.Name, s.Name, N'مصروف عام'),
                    p.PaymentDate = p.CreatedAt,
                    p.Reason = N'دفعة'
                FROM [Payment] p
                LEFT JOIN [Customers] c ON c.Id = p.CustomerId
                LEFT JOIN [Suppliers] s ON s.Id = p.SupplierId;
                """);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Employees_EmployeeId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_EmployeeId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PartyName",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PartyType",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Payment");
        }
    }
}
