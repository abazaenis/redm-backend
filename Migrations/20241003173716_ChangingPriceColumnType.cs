#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class ChangingPriceColumnType : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<double>(
				name: "Price",
				table: "Products",
				type: "double precision",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "integer");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Price",
				table: "Products",
				type: "integer",
				nullable: false,
				oldClrType: typeof(double),
				oldType: "double precision");
		}
	}
}
