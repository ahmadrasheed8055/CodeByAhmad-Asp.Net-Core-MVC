using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMind_API.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetUserId = table.Column<int>(type: "int", nullable: false),
                    ActorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActorImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_AppNotifications_AppUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_TargetUserId",
                table: "AppNotifications",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNotifications");
        }
    }
}
