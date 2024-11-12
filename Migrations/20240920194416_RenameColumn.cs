#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class RenameColumn : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Description",
				table: "Stories",
				newName: "Title");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Title",
				table: "Stories",
				newName: "Description");
		}
	}
}
