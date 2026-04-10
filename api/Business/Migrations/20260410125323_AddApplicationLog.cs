using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LogLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Exception = table.Column<string>(type: "TEXT", nullable: true),
                    EventId = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLog", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "7c3525bc-cf6a-4708-8873-012e823be560");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "2ea41900-82ca-4710-9a02-05bb541f51a1");

            migrationBuilder.UpdateData(
                table: "AstronautDetail",
                keyColumn: "Id",
                keyValue: 1,
                column: "CareerStartDate",
                value: new DateTime(2026, 4, 10, 8, 53, 22, 791, DateTimeKind.Local).AddTicks(6127));

            migrationBuilder.UpdateData(
                table: "AstronautDuty",
                keyColumn: "Id",
                keyValue: 1,
                column: "DutyStartDate",
                value: new DateTime(2026, 4, 10, 8, 53, 22, 792, DateTimeKind.Local).AddTicks(7441));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a6ee1b62-892d-4c0e-b3e9-68755d5ee327", new DateTime(2026, 4, 10, 12, 53, 22, 758, DateTimeKind.Utc).AddTicks(5549), "AQAAAAIAAYagAAAAEJJQO1skF9SmBaNF76k+2kZWBjZA37StHr7cWCtlZLeQbN9ndWyqhkjsLGWMOoWI3g==", "71e4948c-0c43-48f1-b62a-7c5bd4593521" });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Person",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId_DutyStartDate",
                table: "AstronautDuty",
                columns: new[] { "PersonId", "DutyStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId_DutyTitle_DutyStartDate",
                table: "AstronautDuty",
                columns: new[] { "PersonId", "DutyTitle", "DutyStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLog_LogLevel",
                table: "ApplicationLog",
                column: "LogLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLog_Timestamp",
                table: "ApplicationLog",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLog");

            migrationBuilder.DropIndex(
                name: "IX_Person_Name",
                table: "Person");

            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId_DutyStartDate",
                table: "AstronautDuty");

            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId_DutyTitle_DutyStartDate",
                table: "AstronautDuty");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "247b7a1c-2324-4bf6-90b5-89bfc2882273");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "e91fac2c-1d70-4a10-a875-22b668654cf7");

            migrationBuilder.UpdateData(
                table: "AstronautDetail",
                keyColumn: "Id",
                keyValue: 1,
                column: "CareerStartDate",
                value: new DateTime(2026, 4, 10, 4, 29, 45, 582, DateTimeKind.Local).AddTicks(7257));

            migrationBuilder.UpdateData(
                table: "AstronautDuty",
                keyColumn: "Id",
                keyValue: 1,
                column: "DutyStartDate",
                value: new DateTime(2026, 4, 10, 4, 29, 45, 584, DateTimeKind.Local).AddTicks(7676));

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "43fa4bb6-6857-41fb-8897-51d95b52499f", new DateTime(2026, 4, 10, 8, 29, 45, 520, DateTimeKind.Utc).AddTicks(568), "AQAAAAIAAYagAAAAEH88PEoZzNa0x0V5B+fhQrR6vSZYd6CVP8WjE1F3bp27q27YCYO72eb8ZIIW/aTVfQ==", "9296bc08-81f1-46b6-a0dc-2118152cdac9" });
        }
    }
}
