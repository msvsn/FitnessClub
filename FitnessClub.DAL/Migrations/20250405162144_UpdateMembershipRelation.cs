using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessClub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMembershipRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeId1",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_MembershipTypeId1",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "MembershipTypeId1",
                table: "Memberships");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MembershipTypeId1",
                table: "Memberships",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_MembershipTypeId1",
                table: "Memberships",
                column: "MembershipTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_MembershipTypes_MembershipTypeId1",
                table: "Memberships",
                column: "MembershipTypeId1",
                principalTable: "MembershipTypes",
                principalColumn: "MembershipTypeId");
        }
    }
}
