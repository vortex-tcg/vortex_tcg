using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "EffectTypes");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "CollectionCard");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "FriendsLists",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "Champions",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "FriendId",
                table: "FriendsLists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FriendId",
                table: "FriendsLists");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "FriendsLists",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Champions",
                newName: "Label");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EffectTypes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Collections",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "CollectionCard",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
