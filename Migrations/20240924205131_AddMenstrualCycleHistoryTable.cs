#nullable disable

namespace Redm_backend.Migrations
{
	using System;

	using Microsoft.EntityFrameworkCore.Migrations;

	using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

	/// <inheritdoc />
	public partial class AddMenstrualCycleHistoryTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PeriodHistory",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					UserId = table.Column<int>(type: "integer", nullable: false),
					StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PeriodHistory", x => x.Id);
					table.ForeignKey(
						name: "FK_PeriodHistory_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PeriodHistory_UserId",
				table: "PeriodHistory",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PeriodHistory");
		}
	}
}
