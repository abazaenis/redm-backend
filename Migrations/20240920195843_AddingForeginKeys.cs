#nullable disable

namespace Redm_backend.Migrations
{
	using Microsoft.EntityFrameworkCore.Migrations;

	/// <inheritdoc />
	public partial class AddingForeginKeys : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
				name: "IX_Stories_PostId",
				table: "Stories",
				column: "PostId");

			migrationBuilder.CreateIndex(
				name: "IX_Posts_PostCategoryId",
				table: "Posts",
				column: "PostCategoryId");

			migrationBuilder.AddForeignKey(
				name: "FK_Posts_PostCategories_PostCategoryId",
				table: "Posts",
				column: "PostCategoryId",
				principalTable: "PostCategories",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_Stories_Posts_PostId",
				table: "Stories",
				column: "PostId",
				principalTable: "Posts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Posts_PostCategories_PostCategoryId",
				table: "Posts");

			migrationBuilder.DropForeignKey(
				name: "FK_Stories_Posts_PostId",
				table: "Stories");

			migrationBuilder.DropIndex(
				name: "IX_Stories_PostId",
				table: "Stories");

			migrationBuilder.DropIndex(
				name: "IX_Posts_PostCategoryId",
				table: "Posts");
		}
	}
}
