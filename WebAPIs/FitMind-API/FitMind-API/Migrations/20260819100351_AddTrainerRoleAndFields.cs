using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerRoleAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certifications",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AppUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SpecializationCategoryId",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "AppUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_SpecializationCategoryId",
                table: "AppUsers",
                column: "SpecializationCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_Categories_SpecializationCategoryId",
                table: "AppUsers",
                column: "SpecializationCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_Categories_SpecializationCategoryId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_SpecializationCategoryId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Availability",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Certifications",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SpecializationCategoryId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "AppUsers");
        }
    }
}
