using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Redm_backend.Migrations
{
    /// <inheritdoc />
    public partial class RenamingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SexualActivitySymtpoms",
                table: "Symptoms",
                newName: "SexualActivitySymptoms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SexualActivitySymptoms",
                table: "Symptoms",
                newName: "SexualActivitySymtpoms");
        }
    }
}
