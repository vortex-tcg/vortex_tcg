using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedingTest2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boosters_Card_CardId",
                table: "Boosters");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_CardType_CardTypeId",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_Extension_ExtensionId",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_Rarity_RarityId",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Champion_EffectChampion_EffectChampionId",
                table: "Champion");

            migrationBuilder.DropForeignKey(
                name: "FK_Champion_Faction_FactionId",
                table: "Champion");

            migrationBuilder.DropForeignKey(
                name: "FK_Class_Card_CardId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCard_Card_CardId",
                table: "CollectionCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Champion_ChampionId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Condition_ConditionType_ConditionTypeId",
                table: "Condition");

            migrationBuilder.DropForeignKey(
                name: "FK_Condition_EffectCard_EffectCardId",
                table: "Condition");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Collections_CollectionId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCard_Decks_DeckId",
                table: "DeckCard");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectCard_Card_CardId",
                table: "EffectCard");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectCard_EffectType_EffectTypeId",
                table: "EffectCard");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectDescription_EffectCard_EffectCardId",
                table: "EffectDescription");

            migrationBuilder.DropForeignKey(
                name: "FK_Faction_Card_CardId",
                table: "Faction");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendsList_Users_UserId",
                table: "FriendsList");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelog_ActionType_ActionTypeId",
                table: "Gamelog");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelog_Users_UserId",
                table: "Gamelog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rarity",
                table: "Rarity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gamelog",
                table: "Gamelog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendsList",
                table: "FriendsList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Faction",
                table: "Faction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Extension",
                table: "Extension");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectType",
                table: "EffectType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectDescription",
                table: "EffectDescription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectChampion",
                table: "EffectChampion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectCard",
                table: "EffectCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeckCard",
                table: "DeckCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConditionType",
                table: "ConditionType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Condition",
                table: "Condition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Class",
                table: "Class");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Champion",
                table: "Champion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardType",
                table: "CardType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Card",
                table: "Card");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionType",
                table: "ActionType");

            migrationBuilder.RenameTable(
                name: "Rarity",
                newName: "Rarities");

            migrationBuilder.RenameTable(
                name: "Gamelog",
                newName: "Gamelogs");

            migrationBuilder.RenameTable(
                name: "FriendsList",
                newName: "FriendsLists");

            migrationBuilder.RenameTable(
                name: "Faction",
                newName: "Factions");

            migrationBuilder.RenameTable(
                name: "Extension",
                newName: "Extensions");

            migrationBuilder.RenameTable(
                name: "EffectType",
                newName: "EffectTypes");

            migrationBuilder.RenameTable(
                name: "EffectDescription",
                newName: "EffectDescriptions");

            migrationBuilder.RenameTable(
                name: "EffectChampion",
                newName: "EffectChampions");

            migrationBuilder.RenameTable(
                name: "EffectCard",
                newName: "EffectCards");

            migrationBuilder.RenameTable(
                name: "DeckCard",
                newName: "DeckCards");

            migrationBuilder.RenameTable(
                name: "ConditionType",
                newName: "ConditionTypes");

            migrationBuilder.RenameTable(
                name: "Condition",
                newName: "Conditions");

            migrationBuilder.RenameTable(
                name: "Class",
                newName: "Classes");

            migrationBuilder.RenameTable(
                name: "Champion",
                newName: "Champions");

            migrationBuilder.RenameTable(
                name: "CardType",
                newName: "CardTypes");

            migrationBuilder.RenameTable(
                name: "Card",
                newName: "Cards");

            migrationBuilder.RenameTable(
                name: "ActionType",
                newName: "ActionTypes");

            migrationBuilder.RenameIndex(
                name: "IX_Gamelog_UserId",
                table: "Gamelogs",
                newName: "IX_Gamelogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Gamelog_ActionTypeId",
                table: "Gamelogs",
                newName: "IX_Gamelogs_ActionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendsList_UserId",
                table: "FriendsLists",
                newName: "IX_FriendsLists_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Faction_CardId",
                table: "Factions",
                newName: "IX_Factions_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectDescription_EffectCardId",
                table: "EffectDescriptions",
                newName: "IX_EffectDescriptions_EffectCardId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectCard_EffectTypeId",
                table: "EffectCards",
                newName: "IX_EffectCards_EffectTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectCard_CardId",
                table: "EffectCards",
                newName: "IX_EffectCards_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_DeckCard_DeckId",
                table: "DeckCards",
                newName: "IX_DeckCards_DeckId");

            migrationBuilder.RenameIndex(
                name: "IX_DeckCard_CollectionId",
                table: "DeckCards",
                newName: "IX_DeckCards_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Condition_EffectCardId",
                table: "Conditions",
                newName: "IX_Conditions_EffectCardId");

            migrationBuilder.RenameIndex(
                name: "IX_Condition_ConditionTypeId",
                table: "Conditions",
                newName: "IX_Conditions_ConditionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Class_CardId",
                table: "Classes",
                newName: "IX_Classes_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_Champion_FactionId",
                table: "Champions",
                newName: "IX_Champions_FactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Champion_EffectChampionId",
                table: "Champions",
                newName: "IX_Champions_EffectChampionId");

            migrationBuilder.RenameIndex(
                name: "IX_Card_RarityId",
                table: "Cards",
                newName: "IX_Cards_RarityId");

            migrationBuilder.RenameIndex(
                name: "IX_Card_ExtensionId",
                table: "Cards",
                newName: "IX_Cards_ExtensionId");

            migrationBuilder.RenameIndex(
                name: "IX_Card_CardTypeId",
                table: "Cards",
                newName: "IX_Cards_CardTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rarities",
                table: "Rarities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gamelogs",
                table: "Gamelogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendsLists",
                table: "FriendsLists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Factions",
                table: "Factions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Extensions",
                table: "Extensions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectTypes",
                table: "EffectTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectDescriptions",
                table: "EffectDescriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectChampions",
                table: "EffectChampions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectCards",
                table: "EffectCards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeckCards",
                table: "DeckCards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConditionTypes",
                table: "ConditionTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conditions",
                table: "Conditions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classes",
                table: "Classes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Champions",
                table: "Champions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardTypes",
                table: "CardTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cards",
                table: "Cards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionTypes",
                table: "ActionTypes",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Label" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Player" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Boosters_Cards_CardId",
                table: "Boosters",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_CardTypes_CardTypeId",
                table: "Cards",
                column: "CardTypeId",
                principalTable: "CardTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Extensions_ExtensionId",
                table: "Cards",
                column: "ExtensionId",
                principalTable: "Extensions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Rarities_RarityId",
                table: "Cards",
                column: "RarityId",
                principalTable: "Rarities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Champions_EffectChampions_EffectChampionId",
                table: "Champions",
                column: "EffectChampionId",
                principalTable: "EffectChampions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Champions_Factions_FactionId",
                table: "Champions",
                column: "FactionId",
                principalTable: "Factions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Cards_CardId",
                table: "Classes",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCard_Cards_CardId",
                table: "CollectionCard",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Champions_ChampionId",
                table: "Collections",
                column: "ChampionId",
                principalTable: "Champions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conditions_ConditionTypes_ConditionTypeId",
                table: "Conditions",
                column: "ConditionTypeId",
                principalTable: "ConditionTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conditions_EffectCards_EffectCardId",
                table: "Conditions",
                column: "EffectCardId",
                principalTable: "EffectCards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_Decks_DeckId",
                table: "DeckCards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EffectCards_Cards_CardId",
                table: "EffectCards",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EffectCards_EffectTypes_EffectTypeId",
                table: "EffectCards",
                column: "EffectTypeId",
                principalTable: "EffectTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EffectDescriptions_EffectCards_EffectCardId",
                table: "EffectDescriptions",
                column: "EffectCardId",
                principalTable: "EffectCards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Factions_Cards_CardId",
                table: "Factions",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendsLists_Users_UserId",
                table: "FriendsLists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelogs_ActionTypes_ActionTypeId",
                table: "Gamelogs",
                column: "ActionTypeId",
                principalTable: "ActionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelogs_Users_UserId",
                table: "Gamelogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Boosters_Cards_CardId",
                table: "Boosters");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_CardTypes_CardTypeId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Extensions_ExtensionId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Rarities_RarityId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Champions_EffectChampions_EffectChampionId",
                table: "Champions");

            migrationBuilder.DropForeignKey(
                name: "FK_Champions_Factions_FactionId",
                table: "Champions");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Cards_CardId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionCard_Cards_CardId",
                table: "CollectionCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Collections_Champions_ChampionId",
                table: "Collections");

            migrationBuilder.DropForeignKey(
                name: "FK_Conditions_ConditionTypes_ConditionTypeId",
                table: "Conditions");

            migrationBuilder.DropForeignKey(
                name: "FK_Conditions_EffectCards_EffectCardId",
                table: "Conditions");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_Collections_CollectionId",
                table: "DeckCards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_Decks_DeckId",
                table: "DeckCards");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectCards_Cards_CardId",
                table: "EffectCards");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectCards_EffectTypes_EffectTypeId",
                table: "EffectCards");

            migrationBuilder.DropForeignKey(
                name: "FK_EffectDescriptions_EffectCards_EffectCardId",
                table: "EffectDescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Factions_Cards_CardId",
                table: "Factions");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendsLists_Users_UserId",
                table: "FriendsLists");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelogs_ActionTypes_ActionTypeId",
                table: "Gamelogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Gamelogs_Users_UserId",
                table: "Gamelogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rarities",
                table: "Rarities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Gamelogs",
                table: "Gamelogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendsLists",
                table: "FriendsLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Factions",
                table: "Factions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Extensions",
                table: "Extensions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectTypes",
                table: "EffectTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectDescriptions",
                table: "EffectDescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectChampions",
                table: "EffectChampions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EffectCards",
                table: "EffectCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeckCards",
                table: "DeckCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConditionTypes",
                table: "ConditionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conditions",
                table: "Conditions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classes",
                table: "Classes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Champions",
                table: "Champions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardTypes",
                table: "CardTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cards",
                table: "Cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActionTypes",
                table: "ActionTypes");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameTable(
                name: "Rarities",
                newName: "Rarity");

            migrationBuilder.RenameTable(
                name: "Gamelogs",
                newName: "Gamelog");

            migrationBuilder.RenameTable(
                name: "FriendsLists",
                newName: "FriendsList");

            migrationBuilder.RenameTable(
                name: "Factions",
                newName: "Faction");

            migrationBuilder.RenameTable(
                name: "Extensions",
                newName: "Extension");

            migrationBuilder.RenameTable(
                name: "EffectTypes",
                newName: "EffectType");

            migrationBuilder.RenameTable(
                name: "EffectDescriptions",
                newName: "EffectDescription");

            migrationBuilder.RenameTable(
                name: "EffectChampions",
                newName: "EffectChampion");

            migrationBuilder.RenameTable(
                name: "EffectCards",
                newName: "EffectCard");

            migrationBuilder.RenameTable(
                name: "DeckCards",
                newName: "DeckCard");

            migrationBuilder.RenameTable(
                name: "ConditionTypes",
                newName: "ConditionType");

            migrationBuilder.RenameTable(
                name: "Conditions",
                newName: "Condition");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Class");

            migrationBuilder.RenameTable(
                name: "Champions",
                newName: "Champion");

            migrationBuilder.RenameTable(
                name: "CardTypes",
                newName: "CardType");

            migrationBuilder.RenameTable(
                name: "Cards",
                newName: "Card");

            migrationBuilder.RenameTable(
                name: "ActionTypes",
                newName: "ActionType");

            migrationBuilder.RenameIndex(
                name: "IX_Gamelogs_UserId",
                table: "Gamelog",
                newName: "IX_Gamelog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Gamelogs_ActionTypeId",
                table: "Gamelog",
                newName: "IX_Gamelog_ActionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendsLists_UserId",
                table: "FriendsList",
                newName: "IX_FriendsList_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Factions_CardId",
                table: "Faction",
                newName: "IX_Faction_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectDescriptions_EffectCardId",
                table: "EffectDescription",
                newName: "IX_EffectDescription_EffectCardId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectCards_EffectTypeId",
                table: "EffectCard",
                newName: "IX_EffectCard_EffectTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_EffectCards_CardId",
                table: "EffectCard",
                newName: "IX_EffectCard_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_DeckCards_DeckId",
                table: "DeckCard",
                newName: "IX_DeckCard_DeckId");

            migrationBuilder.RenameIndex(
                name: "IX_DeckCards_CollectionId",
                table: "DeckCard",
                newName: "IX_DeckCard_CollectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Conditions_EffectCardId",
                table: "Condition",
                newName: "IX_Condition_EffectCardId");

            migrationBuilder.RenameIndex(
                name: "IX_Conditions_ConditionTypeId",
                table: "Condition",
                newName: "IX_Condition_ConditionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_CardId",
                table: "Class",
                newName: "IX_Class_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_Champions_FactionId",
                table: "Champion",
                newName: "IX_Champion_FactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Champions_EffectChampionId",
                table: "Champion",
                newName: "IX_Champion_EffectChampionId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_RarityId",
                table: "Card",
                newName: "IX_Card_RarityId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_ExtensionId",
                table: "Card",
                newName: "IX_Card_ExtensionId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_CardTypeId",
                table: "Card",
                newName: "IX_Card_CardTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rarity",
                table: "Rarity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Gamelog",
                table: "Gamelog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendsList",
                table: "FriendsList",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Faction",
                table: "Faction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Extension",
                table: "Extension",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectType",
                table: "EffectType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectDescription",
                table: "EffectDescription",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectChampion",
                table: "EffectChampion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EffectCard",
                table: "EffectCard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeckCard",
                table: "DeckCard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConditionType",
                table: "ConditionType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Condition",
                table: "Condition",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Class",
                table: "Class",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Champion",
                table: "Champion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardType",
                table: "CardType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Card",
                table: "Card",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActionType",
                table: "ActionType",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Boosters_Card_CardId",
                table: "Boosters",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_CardType_CardTypeId",
                table: "Card",
                column: "CardTypeId",
                principalTable: "CardType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Extension_ExtensionId",
                table: "Card",
                column: "ExtensionId",
                principalTable: "Extension",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Card_Rarity_RarityId",
                table: "Card",
                column: "RarityId",
                principalTable: "Rarity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Champion_EffectChampion_EffectChampionId",
                table: "Champion",
                column: "EffectChampionId",
                principalTable: "EffectChampion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Champion_Faction_FactionId",
                table: "Champion",
                column: "FactionId",
                principalTable: "Faction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Card_CardId",
                table: "Class",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionCard_Card_CardId",
                table: "CollectionCard",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Collections_Champion_ChampionId",
                table: "Collections",
                column: "ChampionId",
                principalTable: "Champion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Condition_ConditionType_ConditionTypeId",
                table: "Condition",
                column: "ConditionTypeId",
                principalTable: "ConditionType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Condition_EffectCard_EffectCardId",
                table: "Condition",
                column: "EffectCardId",
                principalTable: "EffectCard",
                principalColumn: "Id");

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
                name: "FK_EffectCard_Card_CardId",
                table: "EffectCard",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EffectCard_EffectType_EffectTypeId",
                table: "EffectCard",
                column: "EffectTypeId",
                principalTable: "EffectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EffectDescription_EffectCard_EffectCardId",
                table: "EffectDescription",
                column: "EffectCardId",
                principalTable: "EffectCard",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faction_Card_CardId",
                table: "Faction",
                column: "CardId",
                principalTable: "Card",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendsList_Users_UserId",
                table: "FriendsList",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelog_ActionType_ActionTypeId",
                table: "Gamelog",
                column: "ActionTypeId",
                principalTable: "ActionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gamelog_Users_UserId",
                table: "Gamelog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
