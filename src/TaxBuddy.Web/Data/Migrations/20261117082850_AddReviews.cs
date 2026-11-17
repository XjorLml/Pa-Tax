using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reply_text = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    reply_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviews", x => x.id);
                    table.CheckConstraint("ck_reviews_stars", "stars BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_reviews_accountant_id",
                        column: x => x.accountant_id,
                        principalTable: "accountant_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reviews_client_account_id",
                        column: x => x.client_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reviews_accountant_id_client_account_id",
                table: "reviews",
                columns: new[] { "accountant_id", "client_account_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reviews_client_account_id",
                table: "reviews",
                column: "client_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reviews");
        }
    }
}
