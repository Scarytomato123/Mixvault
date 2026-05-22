using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mixvault_API.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackArtistToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayName = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProfilePictureURL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserCreatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserID);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "playlist",
                columns: table => new
                {
                    PlaylistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlaylistName = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistDescription = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistGenre = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistTags = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistArtworkURL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistCreatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    fkUser = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlaylistID);
                    table.ForeignKey(
                        name: "playlist_ibfk_1",
                        column: x => x.fkUser,
                        principalTable: "user",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "track",
                columns: table => new
                {
                    TrackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DurationMs = table.Column<double>(type: "double", nullable: true),
                    TrackFileURL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackGenre = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackTags = table.Column<string>(type: "mediumtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackArtworkURL = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackUploadedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    fkUser = table.Column<int>(type: "int", nullable: false),
                    TrackArtist = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TrackID);
                    table.ForeignKey(
                        name: "track_ibfk_1",
                        column: x => x.fkUser,
                        principalTable: "user",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "userlikesplaylist",
                columns: table => new
                {
                    UserLikesPlaylistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fkUser = table.Column<int>(type: "int", nullable: false),
                    fkPlaylist = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserLikesPlaylistID);
                    table.ForeignKey(
                        name: "userlikesplaylist_ibfk_1",
                        column: x => x.fkUser,
                        principalTable: "user",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "userlikesplaylist_ibfk_2",
                        column: x => x.fkPlaylist,
                        principalTable: "playlist",
                        principalColumn: "PlaylistID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "playlisthastracks",
                columns: table => new
                {
                    PlaylistHasTracksID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Position = table.Column<int>(type: "int", nullable: false),
                    fkTrack = table.Column<int>(type: "int", nullable: false),
                    fkPlaylist = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PlaylistHasTracksID);
                    table.ForeignKey(
                        name: "playlisthastracks_ibfk_1",
                        column: x => x.fkTrack,
                        principalTable: "track",
                        principalColumn: "TrackID");
                    table.ForeignKey(
                        name: "playlisthastracks_ibfk_2",
                        column: x => x.fkPlaylist,
                        principalTable: "playlist",
                        principalColumn: "PlaylistID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "userlikestrack",
                columns: table => new
                {
                    UserLikesTrackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fkUser = table.Column<int>(type: "int", nullable: false),
                    fkTrack = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserLikesTrackID);
                    table.ForeignKey(
                        name: "userlikestrack_ibfk_1",
                        column: x => x.fkUser,
                        principalTable: "user",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "userlikestrack_ibfk_2",
                        column: x => x.fkTrack,
                        principalTable: "track",
                        principalColumn: "TrackID");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "fkUser",
                table: "playlist",
                column: "fkUser");

            migrationBuilder.CreateIndex(
                name: "fkPlaylist",
                table: "playlisthastracks",
                column: "fkPlaylist");

            migrationBuilder.CreateIndex(
                name: "fkTrack",
                table: "playlisthastracks",
                column: "fkTrack");

            migrationBuilder.CreateIndex(
                name: "fkUser1",
                table: "track",
                column: "fkUser");

            migrationBuilder.CreateIndex(
                name: "fkPlaylist1",
                table: "userlikesplaylist",
                column: "fkPlaylist");

            migrationBuilder.CreateIndex(
                name: "fkUser2",
                table: "userlikesplaylist",
                column: "fkUser");

            migrationBuilder.CreateIndex(
                name: "fkTrack1",
                table: "userlikestrack",
                column: "fkTrack");

            migrationBuilder.CreateIndex(
                name: "fkUser3",
                table: "userlikestrack",
                column: "fkUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playlisthastracks");

            migrationBuilder.DropTable(
                name: "userlikesplaylist");

            migrationBuilder.DropTable(
                name: "userlikestrack");

            migrationBuilder.DropTable(
                name: "playlist");

            migrationBuilder.DropTable(
                name: "track");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
