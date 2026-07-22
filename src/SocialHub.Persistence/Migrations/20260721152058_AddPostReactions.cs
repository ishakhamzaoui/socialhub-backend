using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPostReactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_reactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_post_reactions_PostId_Type",
                table: "post_reactions",
                columns: new[] { "PostId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_post_reactions_PostId_UserId",
                table: "post_reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_reactions");
        }
    }
}
