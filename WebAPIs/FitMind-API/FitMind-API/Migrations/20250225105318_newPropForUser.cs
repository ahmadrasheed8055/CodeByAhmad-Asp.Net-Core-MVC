using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class newPropForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookLink",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramLink",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Phone",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePhoto",
                table: "AppUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqueName",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserVisibility",
                table: "AppUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "FacebookLink",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "InstagramLink",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "UniqueName",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "UserVisibility",
                table: "AppUsers");
        }
    }
}
