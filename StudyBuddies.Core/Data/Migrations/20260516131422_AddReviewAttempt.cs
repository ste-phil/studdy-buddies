using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyBuddies.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WordId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCorrect = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserAnswer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Confidence = table.Column<byte>(type: "INTEGER", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewAttempts_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAttempts_UserId_AnsweredAt",
                table: "ReviewAttempts",
                columns: new[] { "UserId", "AnsweredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAttempts_WordId",
                table: "ReviewAttempts",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewAttempts");
        }
    }
}
