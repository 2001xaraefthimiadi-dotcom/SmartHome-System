using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceDeviceToAutomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceDeviceId",
                table: "Automations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Automations_SourceDeviceId",
                table: "Automations",
                column: "SourceDeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Automations_Devices_SourceDeviceId",
                table: "Automations",
                column: "SourceDeviceId",
                principalTable: "Devices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Automations_Devices_SourceDeviceId",
                table: "Automations");

            migrationBuilder.DropIndex(
                name: "IX_Automations_SourceDeviceId",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "SourceDeviceId",
                table: "Automations");
        }
    }
}
