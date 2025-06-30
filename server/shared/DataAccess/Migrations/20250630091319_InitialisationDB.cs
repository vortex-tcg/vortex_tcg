using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialisationDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CardType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConditionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EffectChampion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectChampion", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EffectType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectType", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Extension",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extension", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TurnNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rank",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nbVictory = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rank", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rarity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rarity", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Card",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Hp = table.Column<int>(type: "int", nullable: false),
                    Attack = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Picture = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Effect_active = table.Column<int>(type: "int", nullable: false),
                    RarityId = table.Column<int>(type: "int", nullable: false),
                    ExtensionId = table.Column<int>(type: "int", nullable: false),
                    CardTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Card_CardType_CardTypeId",
                        column: x => x.CardTypeId,
                        principalTable: "CardType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_Extension_ExtensionId",
                        column: x => x.ExtensionId,
                        principalTable: "Extension",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Card_Rarity_RarityId",
                        column: x => x.RarityId,
                        principalTable: "Rarity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Class_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CollectionCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionCard_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EffectCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parameter = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsEquipementEffect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EffectTypeId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EffectCard_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EffectCard_EffectType_EffectTypeId",
                        column: x => x.EffectTypeId,
                        principalTable: "EffectType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Faction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faction_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Condition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConditionDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConditionTypeId = table.Column<int>(type: "int", nullable: true),
                    EffectCardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Condition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Condition_ConditionType_ConditionTypeId",
                        column: x => x.ConditionTypeId,
                        principalTable: "ConditionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Condition_EffectCard_EffectCardId",
                        column: x => x.EffectCardId,
                        principalTable: "EffectCard",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EffectDescription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EffectCardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EffectDescription_EffectCard_EffectCardId",
                        column: x => x.EffectCardId,
                        principalTable: "EffectCard",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Champion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HP = table.Column<int>(type: "int", nullable: false),
                    Picture = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FactionId = table.Column<int>(type: "int", nullable: false),
                    EffectChampionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Champion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Champion_EffectChampion_EffectChampionId",
                        column: x => x.EffectChampionId,
                        principalTable: "EffectChampion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Champion_Faction_FactionId",
                        column: x => x.FactionId,
                        principalTable: "Faction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Collection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CollectionCardId = table.Column<int>(type: "int", nullable: false),
                    ChampionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collection_Champion_ChampionId",
                        column: x => x.ChampionId,
                        principalTable: "Champion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collection_CollectionCard_CollectionCardId",
                        column: x => x.CollectionCardId,
                        principalTable: "CollectionCard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyQuantity = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    RankId = table.Column<int>(type: "int", nullable: false),
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Rank_RankId",
                        column: x => x.RankId,
                        principalTable: "Rank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Booster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booster_Card_CardId",
                        column: x => x.CardId,
                        principalTable: "Card",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Booster_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Deck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deck_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FriendsList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendsList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendsList_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Gamelog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ActionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gamelog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gamelog_ActionType_ActionTypeId",
                        column: x => x.ActionTypeId,
                        principalTable: "ActionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gamelog_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DeckCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    DeckId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckCard_Collection_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckCard_Deck_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Deck",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Booster_CardId",
                table: "Booster",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Booster_UserId",
                table: "Booster",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_CardTypeId",
                table: "Card",
                column: "CardTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_ExtensionId",
                table: "Card",
                column: "ExtensionId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_RarityId",
                table: "Card",
                column: "RarityId");

            migrationBuilder.CreateIndex(
                name: "IX_Champion_EffectChampionId",
                table: "Champion",
                column: "EffectChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_Champion_FactionId",
                table: "Champion",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_CardId",
                table: "Class",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_ChampionId",
                table: "Collection",
                column: "ChampionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collection_CollectionCardId",
                table: "Collection",
                column: "CollectionCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionCard_CardId",
                table: "CollectionCard",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Condition_ConditionTypeId",
                table: "Condition",
                column: "ConditionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Condition_EffectCardId",
                table: "Condition",
                column: "EffectCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Deck_UserId",
                table: "Deck",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCard_CollectionId",
                table: "DeckCard",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCard_DeckId",
                table: "DeckCard",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_EffectCard_CardId",
                table: "EffectCard",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_EffectCard_EffectTypeId",
                table: "EffectCard",
                column: "EffectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EffectDescription_EffectCardId",
                table: "EffectDescription",
                column: "EffectCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Faction_CardId",
                table: "Faction",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendsList_UserId",
                table: "FriendsList",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Gamelog_ActionTypeId",
                table: "Gamelog",
                column: "ActionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Gamelog_UserId",
                table: "Gamelog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CollectionId",
                table: "User",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_User_GameId",
                table: "User",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RankId",
                table: "User",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booster");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Condition");

            migrationBuilder.DropTable(
                name: "DeckCard");

            migrationBuilder.DropTable(
                name: "EffectDescription");

            migrationBuilder.DropTable(
                name: "FriendsList");

            migrationBuilder.DropTable(
                name: "Gamelog");

            migrationBuilder.DropTable(
                name: "ConditionType");

            migrationBuilder.DropTable(
                name: "Deck");

            migrationBuilder.DropTable(
                name: "EffectCard");

            migrationBuilder.DropTable(
                name: "ActionType");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "EffectType");

            migrationBuilder.DropTable(
                name: "Collection");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "Rank");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Champion");

            migrationBuilder.DropTable(
                name: "CollectionCard");

            migrationBuilder.DropTable(
                name: "EffectChampion");

            migrationBuilder.DropTable(
                name: "Faction");

            migrationBuilder.DropTable(
                name: "Card");

            migrationBuilder.DropTable(
                name: "CardType");

            migrationBuilder.DropTable(
                name: "Extension");

            migrationBuilder.DropTable(
                name: "Rarity");
        }
    }
}
