using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrDoggy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupPostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupPostStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    UploaddAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPostStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPostStatuses_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupPostStatuses_AspNetUsers_ModId",
                        column: x => x.ModId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupPostStatuses_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPostStatuses_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostStatuses_AuthorId",
                table: "GroupPostStatuses",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostStatuses_GroupId",
                table: "GroupPostStatuses",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostStatuses_ModId",
                table: "GroupPostStatuses",
                column: "ModId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostStatuses_PostId_GroupId",
                table: "GroupPostStatuses",
                columns: new[] { "PostId", "GroupId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPostStatuses");
        }
    }
}
