using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class FixPollVoteIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PollVotes_PollId_UserId",
                table: "PollVotes");

            migrationBuilder.CreateIndex(
                name: "IX_PollVotes_PollId_UserId_OptionId",
                table: "PollVotes",
                columns: new[] { "PollId", "UserId", "OptionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PollVotes_PollId_UserId_OptionId",
                table: "PollVotes");

            migrationBuilder.CreateIndex(
                name: "IX_PollVotes_PollId_UserId",
                table: "PollVotes",
                columns: new[] { "PollId", "UserId" },
                unique: true);
        }
    }
}
