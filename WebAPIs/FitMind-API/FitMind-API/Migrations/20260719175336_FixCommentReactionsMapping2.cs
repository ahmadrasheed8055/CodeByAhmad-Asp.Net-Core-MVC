using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentReactionsMapping2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_PostComments_PostCommentsCommentId",
                table: "CommentReactions");

            migrationBuilder.DropIndex(
                name: "IX_CommentReactions_PostCommentsCommentId",
                table: "CommentReactions");

            migrationBuilder.DropColumn(
                name: "PostCommentsCommentId",
                table: "CommentReactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostCommentsCommentId",
                table: "CommentReactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_PostCommentsCommentId",
                table: "CommentReactions",
                column: "PostCommentsCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentReactions_PostComments_PostCommentsCommentId",
                table: "CommentReactions",
                column: "PostCommentsCommentId",
                principalTable: "PostComments",
                principalColumn: "CommentId");
        }
    }
}
