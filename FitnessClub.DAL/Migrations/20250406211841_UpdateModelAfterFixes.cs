using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelAfterFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOneTimePass",
                table: "MembershipTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOneTimePass",
                table: "MembershipTypes");
        }
    }
}
