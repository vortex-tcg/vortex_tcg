using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedingTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booster_Card_CardId",
                table: "Booster");

            migrationBuilder.DropForeignKey(
                name: "FK_Booster_User_UserId",
                table: "Booster");

            migrationBuilder.DropForeignKey(
                name: "FK_Collection_Champion_ChampionId",
                table: "Collection");

            migrationBuilder.DropForeignKey(
                name: "FK_Collection_CollectionCard_CollectionCardId",
                table: "Collection");

            migrationBuilder.DropForeignKey(
                name: "FK_Deck_User_UserId",
                table: "Deck");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Collection_CollectionId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Deck_DeckId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendsList_User_UserId",
                table: "FriendsList");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelog_User_UserId",
                table: "Gamelog");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Collection_CollectionId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Game_GameId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Rank_RankId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_RoleId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rank",
                table: "Rank");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Game",
                table: "Game");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deck",
                table: "Deck");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collection",
                table: "Collection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booster",
                table: "Booster");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Role",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "Rank",
                newName: "Ranks");

            migrationBuilder.RenameTable(
                name: "Game",
                newName: "Games");

            migrationBuilder.RenameTable(
                name: "Deck",
                newName: "Decks");

            migrationBuilder.RenameTable(
                name: "Collection",
                newName: "Collections");

            migrationBuilder.RenameTable(
                name: "Booster",
                newName: "Boosters");

            migrationBuilder.RenameIndex(
                name: "IX_User_RoleId",
                table: "Users",
                newName: "IX_Users_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_User_RankId",
                table: "Users",
                newName: "IX_Users_RankId");

            migrationBuilder.RenameIndex(
                name: "IX_User_GameId",
                table: "Users",
                newName: "IX_Users_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_User_CollectionId",
                table: "Users",
                newName: "IX_Users_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Deck_UserId",
                table: "Decks",
                newName: "IX_Decks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Collection_CollectionCardId",
                table: "Collections",
                newName: "IX_Collections_CollectionCardId");

            migrationBuilder.RenameIndex(
                name: "IX_Collection_ChampionId",
                table: "Collections",
                newName: "IX_Collections_ChampionId");

            migrationBuilder.RenameIndex(
                name: "IX_Booster_UserId",
                table: "Boosters",
                newName: "IX_Boosters_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Booster_CardId",
                table: "Boosters",
                newName: "IX_Boosters_CardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ranks",
                table: "Ranks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Games",
                table: "Games",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Decks",
                table: "Decks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collections",
                table: "Collections",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boosters",
                table: "Boosters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boosters_Card_CardId",
                table: "Boosters",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boosters_Users_UserId",
                table: "Boosters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Champion_ChampionId",
                table: "Collections",
                column: "ChampionId",
                principalTable: "Champion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_CollectionCard_CollectionCardId",
                table: "Collections",
                column: "CollectionCardId",
                principalTable: "CollectionCard",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCard_Collections_CollectionId",
                table: "DeckCard",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCard_Decks_DeckId",
                table: "DeckCard",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Users_UserId",
                table: "Decks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendsList_Users_UserId",
                table: "FriendsList",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelog_Users_UserId",
                table: "Gamelog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Collections_CollectionId",
                table: "Users",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Games_GameId",
                table: "Users",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Ranks_RankId",
                table: "Users",
                column: "RankId",
                principalTable: "Ranks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boosters_Card_CardId",
                table: "Boosters");

            migrationBuilder.DropForeignKey(
                name: "FK_Boosters_Users_UserId",
                table: "Boosters");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Champion_ChampionId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_CollectionCard_CollectionCardId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Collections_CollectionId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Decks_DeckId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Users_UserId",
                table: "Decks");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendsList_Users_UserId",
                table: "FriendsList");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelog_Users_UserId",
                table: "Gamelog");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Collections_CollectionId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Games_GameId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Ranks_RankId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ranks",
                table: "Ranks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Games",
                table: "Games");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Decks",
                table: "Decks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Collections",
                table: "Collections");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Boosters",
                table: "Boosters");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Role");

            migrationBuilder.RenameTable(
                name: "Ranks",
                newName: "Rank");

            migrationBuilder.RenameTable(
                name: "Games",
                newName: "Game");

            migrationBuilder.RenameTable(
                name: "Decks",
                newName: "Deck");

            migrationBuilder.RenameTable(
                name: "Collections",
                newName: "Collection");

            migrationBuilder.RenameTable(
                name: "Boosters",
                newName: "Booster");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleId",
                table: "User",
                newName: "IX_User_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RankId",
                table: "User",
                newName: "IX_User_RankId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_GameId",
                table: "User",
                newName: "IX_User_GameId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_CollectionId",
                table: "User",
                newName: "IX_User_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Decks_UserId",
                table: "Deck",
                newName: "IX_Deck_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_CollectionCardId",
                table: "Collection",
                newName: "IX_Collection_CollectionCardId");

            migrationBuilder.RenameIndex(
                name: "IX_Collections_ChampionId",
                table: "Collection",
                newName: "IX_Collection_ChampionId");

            migrationBuilder.RenameIndex(
                name: "IX_Boosters_UserId",
                table: "Booster",
                newName: "IX_Booster_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Boosters_CardId",
                table: "Booster",
                newName: "IX_Booster_CardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rank",
                table: "Rank",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Game",
                table: "Game",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deck",
                table: "Deck",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Collection",
                table: "Collection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booster",
                table: "Booster",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Booster_Card_CardId",
                table: "Booster",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Booster_User_UserId",
                table: "Booster",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Collection_Champion_ChampionId",
                table: "Collection",
                column: "ChampionId",
                principalTable: "Champion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Collection_CollectionCard_CollectionCardId",
                table: "Collection",
                column: "CollectionCardId",
                principalTable: "CollectionCard",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Deck_User_UserId",
                table: "Deck",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCard_Collection_CollectionId",
                table: "DeckCard",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCard_Deck_DeckId",
                table: "DeckCard",
                column: "DeckId",
                principalTable: "Deck",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendsList_User_UserId",
                table: "FriendsList",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelog_User_UserId",
                table: "Gamelog",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Collection_CollectionId",
                table: "User",
                column: "CollectionId",
                principalTable: "Collection",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Game_GameId",
                table: "User",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Rank_RankId",
                table: "User",
                column: "RankId",
                principalTable: "Rank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_RoleId",
                table: "User",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
