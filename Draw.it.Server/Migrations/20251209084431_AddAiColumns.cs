using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Draw.it.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAiColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_ai",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_ai_player",
                table: "rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_ai",
                table: "users");

            migrationBuilder.DropColumn(
                name: "has_ai_player",
                table: "rooms");
        }
    }
}
