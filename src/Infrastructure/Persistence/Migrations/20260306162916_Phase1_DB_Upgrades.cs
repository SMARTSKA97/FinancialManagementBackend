using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_DB_Upgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "transactions",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "transactions",
                table: "Transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "transactions",
                table: "TransactionCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "transactions",
                table: "TransactionCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "feedback",
                table: "Feedbacks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "feedback",
                table: "Feedbacks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "accounts",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "accounts",
                table: "AccountCategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "accounts",
                table: "AccountCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId_Date",
                schema: "transactions",
                table: "Transactions",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Transaction_Amount_Positive",
                schema: "transactions",
                table: "Transactions",
                sql: "\"Amount\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId_Date",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Transaction_Amount_Positive",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "transactions",
                table: "TransactionCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "transactions",
                table: "TransactionCategories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "feedback",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "feedback",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "accounts",
                table: "AccountCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "accounts",
                table: "AccountCategories");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId",
                schema: "transactions",
                table: "Transactions",
                column: "AccountId");
        }
    }
}
