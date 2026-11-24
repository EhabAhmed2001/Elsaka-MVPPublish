using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElsakaHerbs.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddWebsiteStatusToSiteContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWebsiteEnabled",
                table: "SiteContents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceMessage",
                table: "SiteContents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWebsiteEnabled",
                table: "SiteContents");

            migrationBuilder.DropColumn(
                name: "MaintenanceMessage",
                table: "SiteContents");
        }
    }
}
