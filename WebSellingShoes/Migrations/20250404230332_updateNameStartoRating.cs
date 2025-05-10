using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSellingShoes.Migrations
{
    /// <inheritdoc />
    public partial class updateNameStartoRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Ratings",
                newName: "Star");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings");

            migrationBuilder.RenameColumn(
                name: "Star",
                table: "Ratings",
                newName: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProductId",
                table: "Ratings",
                column: "ProductId",
                unique: true);
        }
    }
}
