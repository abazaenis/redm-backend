using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redm_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedAppleIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppleId",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppleId",
                table: "Users");
        }
    }
}
