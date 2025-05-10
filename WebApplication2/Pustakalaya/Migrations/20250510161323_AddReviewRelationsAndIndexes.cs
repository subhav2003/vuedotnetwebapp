using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pustakalaya.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewRelationsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Books_BookId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BookId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_MemberId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MemberId_BookId",
                table: "Reviews",
                columns: new[] { "MemberId", "BookId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_MemberId_BookId",
                table: "Reviews");

            migrationBuilder.AddColumn<long>(
                name: "BookId1",
                table: "Reviews",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookId1",
                table: "Reviews",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MemberId",
                table: "Reviews",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Books_BookId1",
                table: "Reviews",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "Id");
        }
    }
}
