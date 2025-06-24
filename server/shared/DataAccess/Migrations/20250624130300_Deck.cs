using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Deck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Deck",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deck_UserId",
                table: "Deck",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deck_Users_UserId",
                table: "Deck",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deck_Users_UserId",
                table: "Deck");

            migrationBuilder.DropIndex(
                name: "IX_Deck_UserId",
                table: "Deck");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Deck");

            migrationBuilder.CreateTable(
                name: "DeckUser",
                columns: table => new
                {
                    DeckId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckUser", x => new { x.DeckId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_DeckUser_Deck_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Deck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DeckUser_UsersId",
                table: "DeckUser",
                column: "UsersId");
        }
    }
}
