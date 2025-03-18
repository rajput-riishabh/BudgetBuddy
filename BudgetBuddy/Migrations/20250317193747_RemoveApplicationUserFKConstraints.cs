using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetBuddy.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApplicationUserFKConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
        name: "FK_PasswordResets_ApplicationUsers_UserId",
        table: "PasswordResets");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_ApplicationUsers_ApplicationUserUserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_ApplicationUsers_ApplicationUserUserId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ApplicationUsers_ApplicationUserUserId",
                table: "Expenses");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
