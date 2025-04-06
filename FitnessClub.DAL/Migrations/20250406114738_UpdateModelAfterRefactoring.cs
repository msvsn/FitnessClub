using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelAfterRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPool",
                table: "Clubs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPool",
                table: "Clubs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
