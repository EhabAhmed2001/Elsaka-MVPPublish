using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElsakaHerbs.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientsImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IngredientsImageUrl",
                table: "SiteContents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsImageUrl",
                table: "SiteContents");
        }
    }
}
