using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class paymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeSalaryHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalaryHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalaryHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "f98e685a-b9ee-437f-b219-de2ca94941e6", new DateTime(2026, 4, 13, 20, 50, 5, 34, DateTimeKind.Utc).AddTicks(9003), "AQAAAAIAAYagAAAAEDf6Rt0EcNBixQuZD5yhtl0WkJbMygb80AmQy2giaM9qaogbNDhLXltV+ZmohlaWPQ==", "574c46f8-7a29-408e-8f53-2987ed11b7a1", new DateTime(2026, 4, 13, 20, 50, 5, 34, DateTimeKind.Utc).AddTicks(9005) });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalaryHistories_EmployeeId_EffectiveFrom",
                table: "EmployeeSalaryHistories",
                columns: new[] { "EmployeeId", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSalaryHistories");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "5cf10586-ffa0-4451-b2b3-8a09d3dd3f4e", new DateTime(2026, 4, 13, 12, 25, 40, 326, DateTimeKind.Utc).AddTicks(2813), "AQAAAAIAAYagAAAAEB6XTvvSinmh7j/2zQN+80JIgAZImNoi9LzxTNv0y/4ZYmvuxvxhpFS4NuO2TrnyPw==", "a579e649-1fc4-46ad-8eaf-fe6d078e4e0d", new DateTime(2026, 4, 13, 12, 25, 40, 326, DateTimeKind.Utc).AddTicks(2816) });
        }
    }
}
