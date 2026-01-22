using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInternalUsage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InternalProductUsages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "f958286d-2bd9-44dd-98b9-ad98ff784729", new DateTime(2026, 1, 20, 22, 1, 46, 260, DateTimeKind.Utc).AddTicks(4882), "AQAAAAIAAYagAAAAEOSsOY3nX+vdsT1lVGpzzikQ53iPMLbt2/9Q5L8TrsXTbRvqzH1Y2MaP+r70izpJXQ==", "0ed0f365-2b45-407c-b47b-e6e6ddd918f9", new DateTime(2026, 1, 20, 22, 1, 46, 260, DateTimeKind.Utc).AddTicks(4886) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InternalProductUsages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "2eef9c8e-2a74-48e2-87a4-021ea30d5227", new DateTime(2026, 1, 20, 21, 39, 55, 679, DateTimeKind.Utc).AddTicks(4112), "AQAAAAIAAYagAAAAEK0yiiYVdWFFfCgg+RFgcfLePs/Ihr1zGYFsHJ0N6Ha4S7wZQEN/Ue2X5q8Bb71yCA==", "ffbe643a-a8ab-41fe-b2ee-e5c60bcce44b", new DateTime(2026, 1, 20, 21, 39, 55, 679, DateTimeKind.Utc).AddTicks(4117) });
        }
    }
}
