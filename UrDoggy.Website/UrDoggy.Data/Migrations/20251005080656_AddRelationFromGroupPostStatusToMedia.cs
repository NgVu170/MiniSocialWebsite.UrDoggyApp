using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrDoggy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationFromGroupPostStatusToMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Posts_PostId1",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_PostId1",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "Reports");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Media",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "GroupPostStatusId",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "GroupReports",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "GroupPostStatuses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "GroupPostStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Media_GroupPostStatusId",
                table: "Media",
                column: "GroupPostStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupReports_UserId",
                table: "GroupReports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupReports_AspNetUsers_UserId",
                table: "GroupReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_GroupPostStatuses_GroupPostStatusId",
                table: "Media",
                column: "GroupPostStatusId",
                principalTable: "GroupPostStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupReports_AspNetUsers_UserId",
                table: "GroupReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_GroupPostStatuses_GroupPostStatusId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_GroupPostStatusId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_GroupReports_UserId",
                table: "GroupReports");

            migrationBuilder.DropColumn(
                name: "GroupPostStatusId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GroupReports");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "GroupPostStatuses");

            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Media",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "GroupPostStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PostId1",
                table: "Reports",
                column: "PostId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Posts_PostId1",
                table: "Reports",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");
        }
    }
}
