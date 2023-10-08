using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnupAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModelsChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsersId",
                table: "Playlist",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_UsersId",
                table: "Playlist",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlist_AspNetUsers_UsersId",
                table: "Playlist",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlist_AspNetUsers_UsersId",
                table: "Playlist");

            migrationBuilder.DropIndex(
                name: "IX_Playlist_UsersId",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "Playlist");
        }
    }
}
