using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeChampionEffectNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Champions_Effects_EffectId",
                table: "Champions");

            migrationBuilder.AlterColumn<Guid>(
                name: "EffectId",
                table: "Champions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Champions_Effects_EffectId",
                table: "Champions",
                column: "EffectId",
                principalTable: "Effects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Champions_Effects_EffectId",
                table: "Champions");

            migrationBuilder.AlterColumn<Guid>(
                name: "EffectId",
                table: "Champions",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Champions_Effects_EffectId",
                table: "Champions",
                column: "EffectId",
                principalTable: "Effects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
