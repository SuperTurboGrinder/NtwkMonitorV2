using Microsoft.EntityFrameworkCore.Migrations;

namespace NativeClient.WebAPI.Migrations
{
    public partial class RealTimePingUISettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RealTimePingUIUpdate",
                table: "Profiles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealTimePingUIUpdate",
                table: "Profiles");
        }
    }
}
