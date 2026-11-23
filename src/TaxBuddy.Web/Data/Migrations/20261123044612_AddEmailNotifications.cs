using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "email_notifications",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true); // F14: existing accounts start with emails on, as new ones do (R08).

            migrationBuilder.CreateTable(
                name: "email_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_jobs_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_email_jobs_recipient_account_id",
                        column: x => x.recipient_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_email_jobs_conversation_id",
                table: "email_jobs",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_jobs_recipient_account_id_conversation_id_created_at",
                table: "email_jobs",
                columns: new[] { "recipient_account_id", "conversation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_email_jobs_status_due_at",
                table: "email_jobs",
                columns: new[] { "status", "due_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_jobs");

            migrationBuilder.DropColumn(
                name: "email_notifications",
                table: "users");
        }
    }
}
