#nullable disable

namespace Redm_backend.Migrations
{
	using System;
	using System.Collections.Generic;

	using Microsoft.EntityFrameworkCore.Migrations;

	using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

	/// <inheritdoc />
	public partial class Symtpoms : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Symptoms",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					UserId = table.Column<int>(type: "integer", nullable: false),
					Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					PhysicalSymptoms = table.Column<List<string>>(type: "text[]", nullable: false),
					MoodSymptoms = table.Column<List<string>>(type: "text[]", nullable: false),
					SexualActivitySymtpoms = table.Column<List<string>>(type: "text[]", nullable: false),
					OtherSymptoms = table.Column<List<string>>(type: "text[]", nullable: false),
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Symptoms", x => x.Id);
					table.ForeignKey(
						name: "FK_Symptoms_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Symptoms_UserId",
				table: "Symptoms",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Symptoms");
		}
	}
}
