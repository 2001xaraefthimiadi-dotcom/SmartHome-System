using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeBasedAutomations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutedAt",
                table: "Automations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ScheduledTime",
                table: "Automations",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastExecutedAt",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "ScheduledTime",
                table: "Automations");
        }
    }
}
