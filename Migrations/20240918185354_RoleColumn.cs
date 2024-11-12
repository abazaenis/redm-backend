#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class RoleColumn : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "Role",
				table: "Users",
				type: "text",
				nullable: false,
				defaultValue: "");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Role",
				table: "Users");
		}
	}
}
