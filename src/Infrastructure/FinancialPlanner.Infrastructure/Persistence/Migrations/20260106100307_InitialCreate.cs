using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinancialPlanner.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentSessionId",
                schema: "identity",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownIp",
                schema: "identity",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastKnownUserAgent",
                schema: "identity",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginTime",
                schema: "identity",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenUtc",
                schema: "identity",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DataHash",
                schema: "transactions",
                table: "Transactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogHash",
                schema: "transactions",
                table: "TransactionLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogHash",
                schema: "transactions",
                table: "TransactionCategoryLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogHash",
                schema: "feedback",
                table: "FeedbackLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataHash",
                schema: "accounts",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogHash",
                schema: "accounts",
                table: "AccountLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogHash",
                schema: "accounts",
                table: "AccountCategoryLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByIp = table.Column<string>(type: "text", nullable: false),
                    RevokedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByIp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "CurrentSessionId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastKnownIp",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastKnownUserAgent",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginTime",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSeenUtc",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DataHash",
                schema: "transactions",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LogHash",
                schema: "transactions",
                table: "TransactionLogs");

            migrationBuilder.DropColumn(
                name: "LogHash",
                schema: "transactions",
                table: "TransactionCategoryLogs");

            migrationBuilder.DropColumn(
                name: "LogHash",
                schema: "feedback",
                table: "FeedbackLogs");

            migrationBuilder.DropColumn(
                name: "DataHash",
                schema: "accounts",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LogHash",
                schema: "accounts",
                table: "AccountLogs");

            migrationBuilder.DropColumn(
                name: "LogHash",
                schema: "accounts",
                table: "AccountCategoryLogs");
        }
    }
}
