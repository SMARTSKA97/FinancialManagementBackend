using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancialPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryEntityAndLinkToTransaction1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                schema: "finance",
                table: "transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Category",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_CategoryId",
                schema: "finance",
                table: "transactions",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_transactions_Category_CategoryId",
                schema: "finance",
                table: "transactions",
                column: "CategoryId",
                principalSchema: "identity",
                principalTable: "Category",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transactions_Category_CategoryId",
                schema: "finance",
                table: "transactions");

            migrationBuilder.DropTable(
                name: "Category",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_transactions_CategoryId",
                schema: "finance",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "finance",
                table: "transactions");
        }
    }
}
