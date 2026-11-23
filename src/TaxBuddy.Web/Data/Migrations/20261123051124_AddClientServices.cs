using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saved_accountants",
                columns: table => new
                {
                    client_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    saved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saved_accountants", x => new { x.client_account_id, x.profile_id });
                    table.ForeignKey(
                        name: "fk_saved_accountants_client_account_id",
                        column: x => x.client_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_saved_accountants_profile_id",
                        column: x => x.profile_id,
                        principalTable: "accountant_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_engagements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accountant_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    phase = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_engagements", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_engagements_accountant_account_id",
                        column: x => x.accountant_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_engagements_client_account_id",
                        column: x => x.client_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engagement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_tasks_engagement_id",
                        column: x => x.engagement_id,
                        principalTable: "service_engagements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_updates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engagement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_note = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_updates", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_updates_author_account_id",
                        column: x => x.author_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_updates_engagement_id",
                        column: x => x.engagement_id,
                        principalTable: "service_engagements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_saved_accountants_profile_id",
                table: "saved_accountants",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_engagements_accountant_account_id",
                table: "service_engagements",
                column: "accountant_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_engagements_client_account_id_accountant_account_id~",
                table: "service_engagements",
                columns: new[] { "client_account_id", "accountant_account_id", "service_name" },
                unique: true,
                filter: "phase <> 'Declined'");

            migrationBuilder.CreateIndex(
                name: "ix_service_tasks_engagement_id",
                table: "service_tasks",
                column: "engagement_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_updates_author_account_id",
                table: "service_updates",
                column: "author_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_updates_engagement_id_created_at",
                table: "service_updates",
                columns: new[] { "engagement_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saved_accountants");

            migrationBuilder.DropTable(
                name: "service_tasks");

            migrationBuilder.DropTable(
                name: "service_updates");

            migrationBuilder.DropTable(
                name: "service_engagements");
        }
    }
}
