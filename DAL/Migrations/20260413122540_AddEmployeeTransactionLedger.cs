using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTransactionLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountChanged = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTransactions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeTransactions_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "5cf10586-ffa0-4451-b2b3-8a09d3dd3f4e", new DateTime(2026, 4, 13, 12, 25, 40, 326, DateTimeKind.Utc).AddTicks(2813), "AQAAAAIAAYagAAAAEB6XTvvSinmh7j/2zQN+80JIgAZImNoi9LzxTNv0y/4ZYmvuxvxhpFS4NuO2TrnyPw==", "a579e649-1fc4-46ad-8eaf-fe6d078e4e0d", new DateTime(2026, 4, 13, 12, 25, 40, 326, DateTimeKind.Utc).AddTicks(2816) });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransactions_EmployeeId",
                table: "EmployeeTransactions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTransactions_InvoiceId",
                table: "EmployeeTransactions",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeTransactions");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Employees");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "450f0a49-d4e0-42f7-919f-c5eca671828d", new DateTime(2026, 4, 13, 11, 7, 9, 533, DateTimeKind.Utc).AddTicks(7185), "AQAAAAIAAYagAAAAENiVYBYwgik2S6pncIzSnxawli5JuJ1q8l7rktzryFnNoZQd3U3moh5GFwr0NEQmrQ==", "d9553d1d-90e5-43e0-a406-6baaca2135b8", new DateTime(2026, 4, 13, 11, 7, 9, 533, DateTimeKind.Utc).AddTicks(7188) });
        }
    }
}
