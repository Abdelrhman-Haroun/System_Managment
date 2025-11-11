using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateUserDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash" },
                values: new object[] { "bc57d436-8316-43e8-b856-355da8704992", new DateTime(2025, 11, 9, 10, 40, 21, 150, DateTimeKind.Local).AddTicks(1124), "AQAAAAIAAYagAAAAEEcL1JwedYxocGIY+CjddAu8MO1zP99wicGqCoAiOauakuytPqj0wMf/MfcOHEmHGw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash" },
                values: new object[] { "509a6d04-0f8c-4409-ae04-46de038c316f", new DateTime(2024, 1, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "AQAAAAIAAYagAAAAEK2poOzvN2zUZMLL6QPe2VEpJYad8s4T84uGXw/OVgjCCpQOC5E+q4T5cUmT5ZpZKg==" });
        }
    }
}
