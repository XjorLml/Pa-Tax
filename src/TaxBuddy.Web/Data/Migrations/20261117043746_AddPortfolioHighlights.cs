using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioHighlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accountant_highlights",
                columns: table => new
                {
                    accountant_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    client_type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    work = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    result = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accountant_highlights", x => new { x.accountant_profile_id, x.id });
                    table.ForeignKey(
                        name: "fk_accountant_highlights_accountant_profile_id",
                        column: x => x.accountant_profile_id,
                        principalTable: "accountant_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountant_highlights");
        }
    }
}
