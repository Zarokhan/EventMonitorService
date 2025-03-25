using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventMonitorService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WinEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WinEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDbEventInstance_Created",
                table: "WinEvents",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WinEvents");
        }
    }
}
