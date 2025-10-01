using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrDoggy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReportNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPostStatuses_Posts_PostId",
                table: "GroupPostStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Groups_GroupId",
                table: "Posts");

            migrationBuilder.AddColumn<int>(
                name: "PostId1",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PostId1",
                table: "Reports",
                column: "PostId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPostStatuses_Posts_PostId",
                table: "GroupPostStatuses",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Groups_GroupId",
                table: "Posts",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Posts_PostId",
                table: "Reports",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Posts_PostId1",
                table: "Reports",
                column: "PostId1",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPostStatuses_Posts_PostId",
                table: "GroupPostStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Groups_GroupId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_ReporterId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Posts_PostId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Posts_PostId1",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_PostId1",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PostId1",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reports");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPostStatuses_Posts_PostId",
                table: "GroupPostStatuses",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Groups_GroupId",
                table: "Posts",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
