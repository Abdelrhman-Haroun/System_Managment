using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInternalUsage3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "InternalProductUsages",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "98a13c7d-d625-4ac5-925d-aac0180bc95d", new DateTime(2026, 1, 20, 22, 37, 19, 515, DateTimeKind.Utc).AddTicks(1992), "AQAAAAIAAYagAAAAECzyyIuejIht8Xp5Es/eX5RiymEVNCZoY+JBoOkGmPg4BUnFvs5AXFh0RwEZPIW64g==", "b01d6b47-7a90-4db4-8a98-861e5e8c9ed6", new DateTime(2026, 1, 20, 22, 37, 19, 515, DateTimeKind.Utc).AddTicks(1994) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "InternalProductUsages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "f958286d-2bd9-44dd-98b9-ad98ff784729", new DateTime(2026, 1, 20, 22, 1, 46, 260, DateTimeKind.Utc).AddTicks(4882), "AQAAAAIAAYagAAAAEOSsOY3nX+vdsT1lVGpzzikQ53iPMLbt2/9Q5L8TrsXTbRvqzH1Y2MaP+r70izpJXQ==", "0ed0f365-2b45-407c-b47b-e6e6ddd918f9", new DateTime(2026, 1, 20, 22, 1, 46, 260, DateTimeKind.Utc).AddTicks(4886) });
        }
    }
}
