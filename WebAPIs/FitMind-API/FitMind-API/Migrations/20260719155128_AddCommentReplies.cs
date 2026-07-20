using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "PostComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostCommentsCommentId",
                table: "CommentReactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_ParentCommentId",
                table: "PostComments",
                column: "ParentCommentId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_PostComments_ParentCommentId",
                table: "PostComments",
                column: "ParentCommentId",
                principalTable: "PostComments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentReactions_PostComments_PostCommentsCommentId",
                table: "CommentReactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_PostComments_ParentCommentId",
                table: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_ParentCommentId",
                table: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_CommentReactions_PostCommentsCommentId",
                table: "CommentReactions");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "PostCommentsCommentId",
                table: "CommentReactions");
        }
    }
}
