using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pustakalaya.Migrations
{
    /// <inheritdoc />
    public partial class Announementupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_MemberId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_UserId",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Announcements",
                newName: "MemberId1");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_UserId",
                table: "Announcements",
                newName: "IX_Announcements_MemberId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Members_MemberId",
                table: "Announcements",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Members_MemberId1",
                table: "Announcements",
                column: "MemberId1",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_MemberId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_MemberId1",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "MemberId1",
                table: "Announcements",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_MemberId1",
                table: "Announcements",
                newName: "IX_Announcements_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Members_MemberId",
                table: "Announcements",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Members_UserId",
                table: "Announcements",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
