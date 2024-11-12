#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class UpdatingUsers : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Username",
				table: "Users",
				type: "text",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "text");

			migrationBuilder.AlterColumn<byte[]>(
				name: "PasswordSalt",
				table: "Users",
				type: "bytea",
				nullable: true,
				oldClrType: typeof(byte[]),
				oldType: "bytea");

			migrationBuilder.AlterColumn<byte[]>(
				name: "PasswordHash",
				table: "Users",
				type: "bytea",
				nullable: true,
				oldClrType: typeof(byte[]),
				oldType: "bytea");

			migrationBuilder.AddColumn<string>(
				name: "FirstName",
				table: "Users",
				type: "text",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "GoogleId",
				table: "Users",
				type: "text",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "LastName",
				table: "Users",
				type: "text",
				nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "FirstName",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "GoogleId",
				table: "Users");

			migrationBuilder.DropColumn(
				name: "LastName",
				table: "Users");

			migrationBuilder.AlterColumn<string>(
				name: "Username",
				table: "Users",
				type: "text",
				nullable: false,
				defaultValue: "",
				oldClrType: typeof(string),
				oldType: "text",
				oldNullable: true);

			migrationBuilder.AlterColumn<byte[]>(
				name: "PasswordSalt",
				table: "Users",
				type: "bytea",
				nullable: false,
				defaultValue: new byte[0],
				oldClrType: typeof(byte[]),
				oldType: "bytea",
				oldNullable: true);

			migrationBuilder.AlterColumn<byte[]>(
				name: "PasswordHash",
				table: "Users",
				type: "bytea",
				nullable: false,
				defaultValue: new byte[0],
				oldClrType: typeof(byte[]),
				oldType: "bytea",
				oldNullable: true);
		}
	}
}
