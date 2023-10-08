using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurnupAPI.Migrations
{
    /// <inheritdoc />
    public partial class AllUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Album");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Picture",
                table: "Album",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
