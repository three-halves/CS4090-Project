using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CS4090Project.Migrations
{
    /// <inheritdoc />
    public partial class FirstAndLastPossibleDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    DaysOfTheWeek = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mask = table.Column<string>(type: "TEXT", nullable: false),
                    Privacy = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalizedStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinalizedEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FirstPossibleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastPossibleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Users_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Availability = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => new { x.EventId, x.UserId });
                    table.ForeignKey(
                        name: "FK_Attendance_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendance_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "PasswordHash", "Username" },
                values: new object[] { new Guid("fc771b9e-2a04-42a6-b73a-714d6ddc3feb"), "Test User", "3AA1154717C6DCAA64174FF0394F0D69A7CCA59B2E322C8883E9B7E6FC2D3D048A895858101CA2C281C5483A2B8926C020D1CEB4B74E4B51EE333320E3928390", "testuser" });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "DaysOfTheWeek", "Description", "FinalizedEnd", "FinalizedStart", "FirstPossibleDate", "LastPossibleDate", "Mask", "OrganizerId", "Privacy", "Title" },
                values: new object[] { new Guid("4ec07ab7-5385-475c-b727-3bf5beda74ed"), false, "Test Description", null, null, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1970, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), "[]", new Guid("fc771b9e-2a04-42a6-b73a-714d6ddc3feb"), 2, "Test Event" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_UserId",
                table: "Attendance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerId",
                table: "Events",
                column: "OrganizerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
