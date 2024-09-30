using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redm_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarName",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarName",
                table: "Users");
        }
    }
}
