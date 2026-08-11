using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPollConfigFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowUserOptions",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowVoteEdit",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMultipleChoice",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowUserOptions",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "AllowVoteEdit",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "IsMultipleChoice",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Polls");
        }
    }
}
