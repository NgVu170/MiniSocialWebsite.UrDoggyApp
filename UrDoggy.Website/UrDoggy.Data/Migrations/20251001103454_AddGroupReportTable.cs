using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrDoggy.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupPostId = table.Column<int>(type: "int", nullable: false),
                    ReporterId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupReports_AspNetUsers_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupReports_Posts_GroupPostId",
                        column: x => x.GroupPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupReports_GroupPostId_ReporterId_CreatedAt",
                table: "GroupReports",
                columns: new[] { "GroupPostId", "ReporterId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupReports_ReporterId",
                table: "GroupReports",
                column: "ReporterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupReports");
        }
    }
}
