using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class AddShowResultsBeforeVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowResultsBeforeVoting",
                table: "Polls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowResultsBeforeVoting",
                table: "Polls");
        }
    }
}
