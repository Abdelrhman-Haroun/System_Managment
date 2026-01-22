using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInternalUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InternalProductUsages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id-12345",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp", "UpdateAt" },
                values: new object[] { "7b39de96-d257-4a26-95bc-de74d4eb72e4", new DateTime(2026, 1, 20, 13, 7, 31, 153, DateTimeKind.Utc).AddTicks(8055), "AQAAAAIAAYagAAAAEGpQJkQxxwGo/Ij3JsaRWa4PHXBg9Fj0dzqqJkxpTbPkG2PSl6YBOu/NWXnQfmiQoQ==", "7b159572-0341-4507-b98b-6a53be5eb5b6", new DateTime(2026, 1, 20, 13, 7, 31, 153, DateTimeKind.Utc).AddTicks(8059) });
        }
    }
}
