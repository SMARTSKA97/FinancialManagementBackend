using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTransferCategory",
                schema: "transactions",
                table: "TransactionCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTransferCategory",
                schema: "transactions",
                table: "TransactionCategories");
        }
    }
}
