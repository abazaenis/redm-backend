#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class AddingEmailColumn : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Username",
				table: "Users",
				newName: "Email");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Email",
				table: "Users",
				newName: "Username");
		}
	}
}
