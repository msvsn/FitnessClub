using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCancelledToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Bookings");
        }
    }
}
