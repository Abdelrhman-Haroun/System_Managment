using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodTransactionAndFixInvoiceItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_BankAccount_BankAccountId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_CashBoxes_CashboxId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Customers_CustomerId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_MobileWallet_MobileWalletId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Suppliers_SupplierId",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MobileWallet",
                table: "MobileWallet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankAccount",
                table: "BankAccount");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "MobileWallet",
                newName: "MobileWallets");

            migrationBuilder.RenameTable(
                name: "BankAccount",
                newName: "BankAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_SupplierId",
                table: "Payments",
                newName: "IX_Payments_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_MobileWalletId",
                table: "Payments",
                newName: "IX_Payments_MobileWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_CustomerId",
                table: "Payments",
                newName: "IX_Payments_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_CashboxId",
                table: "Payments",
                newName: "IX_Payments_CashboxId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_BankAccountId",
                table: "Payments",
                newName: "IX_Payments_BankAccountId");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "InvoiceItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBefore",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartyName",
                table: "Payments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartyType",
                table: "Payments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Payments",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Payments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MobileWallets",
                table: "MobileWallets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankAccounts",
                table: "BankAccounts",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PaymentMethodTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    CashboxId = table.Column<int>(type: "int", nullable: true),
                    BankAccountId = table.Column<int>(type: "int", nullable: true),
                    MobileWalletId = table.Column<int>(type: "int", nullable: true),
                    SourceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountChanged = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethodTransactions_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentMethodTransactions_CashBoxes_CashboxId",
                        column: x => x.CashboxId,
                        principalTable: "CashBoxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentMethodTransactions_MobileWallets_MobileWalletId",
                        column: x => x.MobileWalletId,
                        principalTable: "MobileWallets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentMethodTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "450f0a49-d4e0-42f7-919f-c5eca671828d", new DateTime(2026, 4, 13, 11, 7, 9, 533, DateTimeKind.Utc).AddTicks(7185), "AQAAAAIAAYagAAAAENiVYBYwgik2S6pncIzSnxawli5JuJ1q8l7rktzryFnNoZQd3U3moh5GFwr0NEQmrQ==", "d9553d1d-90e5-43e0-a406-6baaca2135b8", new DateTime(2026, 4, 13, 11, 7, 9, 533, DateTimeKind.Utc).AddTicks(7188) });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_EmployeeId",
                table: "Payments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodTransactions_BankAccountId",
                table: "PaymentMethodTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodTransactions_CashboxId",
                table: "PaymentMethodTransactions",
                column: "CashboxId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodTransactions_MobileWalletId",
                table: "PaymentMethodTransactions",
                column: "MobileWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodTransactions_PaymentId",
                table: "PaymentMethodTransactions",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CashBoxes_CashboxId",
                table: "Payments",
                column: "CashboxId",
                principalTable: "CashBoxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Employees_EmployeeId",
                table: "Payments",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MobileWallets_MobileWalletId",
                table: "Payments",
                column: "MobileWalletId",
                principalTable: "MobileWallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Suppliers_SupplierId",
                table: "Payments",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_BankAccounts_BankAccountId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CashBoxes_CashboxId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Customers_CustomerId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Employees_EmployeeId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_MobileWallets_MobileWalletId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Suppliers_SupplierId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentMethodTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_EmployeeId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MobileWallets",
                table: "MobileWallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankAccounts",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PartyName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PartyType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "MobileWallets",
                newName: "MobileWallet");

            migrationBuilder.RenameTable(
                name: "BankAccounts",
                newName: "BankAccount");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_SupplierId",
                table: "Payment",
                newName: "IX_Payment_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_MobileWalletId",
                table: "Payment",
                newName: "IX_Payment_MobileWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CustomerId",
                table: "Payment",
                newName: "IX_Payment_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_CashboxId",
                table: "Payment",
                newName: "IX_Payment_CashboxId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BankAccountId",
                table: "Payment",
                newName: "IX_Payment_BankAccountId");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Payment",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MobileWallet",
                table: "MobileWallet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankAccount",
                table: "BankAccount",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "7c883928-b095-454f-900d-eb54c616665d", new DateTime(2026, 4, 5, 10, 45, 14, 387, DateTimeKind.Utc).AddTicks(3209), "AQAAAAIAAYagAAAAEAi6+9v9ZvlBUJ1U6GYHVTptEIXxW1fD8FLRGyMD/c8pTmKIKahDX0YVU8jjlaBZkg==", "738e99e3-1a2e-4108-9438-aa0fe0082b60", new DateTime(2026, 4, 5, 10, 45, 14, 387, DateTimeKind.Utc).AddTicks(3212) });

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_BankAccount_BankAccountId",
                table: "Payment",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_CashBoxes_CashboxId",
                table: "Payment",
                column: "CashboxId",
                principalTable: "CashBoxes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Customers_CustomerId",
                table: "Payment",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_MobileWallet_MobileWalletId",
                table: "Payment",
                column: "MobileWalletId",
                principalTable: "MobileWallet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Suppliers_SupplierId",
                table: "Payment",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }
    }
}
