using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "post_reposts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPostId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_reposts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalPostId = table.Column<Guid>(type: "uuid", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledForUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "post_hashtags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    HashtagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_hashtags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_hashtags_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_media_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "post_mentions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    MentionedUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_mentions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_post_mentions_posts_PostId",
                        column: x => x.PostId,
                        principalTable: "posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_post_hashtags_HashtagId",
                table: "post_hashtags",
                column: "HashtagId");

            migrationBuilder.CreateIndex(
                name: "IX_post_hashtags_PostId_HashtagId",
                table: "post_hashtags",
                columns: new[] { "PostId", "HashtagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_media_MediaAssetId",
                table: "post_media",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_post_media_PostId",
                table: "post_media",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_post_mentions_MentionedUserId",
                table: "post_mentions",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_post_mentions_PostId_MentionedUserId",
                table: "post_mentions",
                columns: new[] { "PostId", "MentionedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_reposts_OriginalPostId",
                table: "post_reposts",
                column: "OriginalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_post_reposts_UserId_OriginalPostId",
                table: "post_reposts",
                columns: new[] { "UserId", "OriginalPostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_AuthorId",
                table: "posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_AuthorId_Status",
                table: "posts",
                columns: new[] { "AuthorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_posts_OriginalPostId",
                table: "posts",
                column: "OriginalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_posts_ScheduledForUtc",
                table: "posts",
                column: "ScheduledForUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "post_hashtags");

            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.DropTable(
                name: "post_mentions");

            migrationBuilder.DropTable(
                name: "post_reposts");

            migrationBuilder.DropTable(
                name: "posts");
        }
    }
}
