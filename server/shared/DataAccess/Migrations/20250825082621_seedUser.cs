using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class seedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards");

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "DeckCards",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Ranks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 25, 8, 26, 20, 805, DateTimeKind.Utc).AddTicks(46));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 25, 8, 26, 20, 804, DateTimeKind.Utc).AddTicks(9956));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 25, 8, 26, 20, 804, DateTimeKind.Utc).AddTicks(9958));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CollectionId", "CreatedAtUtc", "CreatedBy", "CurrencyQuantity", "Email", "FirstName", "GameId", "Language", "LastName", "Password", "RankId", "RoleId", "UpdatedAt", "UpdatedAtUtc", "UpdatedBy", "Username" },
                values: new object[] { 1, null, new DateTime(2025, 8, 25, 8, 26, 20, 805, DateTimeKind.Utc).AddTicks(62), "System", 1000, "johndoe@gmail.com", "John", null, "fr", "Doe", "Mdp", 1, 2, null, null, null, "johndoe" });

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "CollectionId",
                table: "DeckCards",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Ranks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 19, 9, 53, 57, 678, DateTimeKind.Utc).AddTicks(8460));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 19, 9, 53, 57, 678, DateTimeKind.Utc).AddTicks(8351));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2025, 8, 19, 9, 53, 57, 678, DateTimeKind.Utc).AddTicks(8353));

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
