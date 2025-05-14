using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pustakalaya.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementMemberRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Announcements",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "MemberId",
                table: "Announcements",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_MemberId",
                table: "Announcements",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_UserId",
                table: "Announcements",
                column: "UserId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_MemberId",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Members_UserId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_MemberId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_UserId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Announcements");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Announcements",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
