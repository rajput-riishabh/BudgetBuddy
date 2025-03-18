using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBuddy.Migrations
{
    /// <inheritdoc />
    public partial class DropApplicationUserFKColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
        name: "IX_Categories_ApplicationUserUserId",
        table: "Categories");

            migrationBuilder.DropIndex(
        name: "IX_Budgets_ApplicationUserUserId",
        table: "Budgets");

            migrationBuilder.DropIndex(
        name: "IX_Expenses_ApplicationUserUserId",
        table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ApplicationUserUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserUserId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "ApplicationUserUserId",
                table: "Expenses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
