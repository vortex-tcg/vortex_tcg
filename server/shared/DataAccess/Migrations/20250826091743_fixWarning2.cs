using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fixWarning2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Ranks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 17, 43, 143, DateTimeKind.Utc).AddTicks(5046));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 17, 43, 143, DateTimeKind.Utc).AddTicks(4949));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 17, 43, 143, DateTimeKind.Utc).AddTicks(4952));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 17, 43, 143, DateTimeKind.Utc).AddTicks(5064));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Ranks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 13, 17, 429, DateTimeKind.Utc).AddTicks(468));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 13, 17, 429, DateTimeKind.Utc).AddTicks(372));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 13, 17, 429, DateTimeKind.Utc).AddTicks(374));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 26, 9, 13, 17, 429, DateTimeKind.Utc).AddTicks(485));
        }
    }
}
