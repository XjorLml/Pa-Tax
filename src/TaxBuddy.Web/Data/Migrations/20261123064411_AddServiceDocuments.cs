using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "service_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    engagement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: true),
                    uploader_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    content_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    length = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_files", x => x.id);
                    table.CheckConstraint("ck_service_files_length", "length BETWEEN 1 AND 10485760");
                    table.ForeignKey(
                        name: "fk_service_files_engagement_id",
                        column: x => x.engagement_id,
                        principalTable: "service_engagements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_files_task_id",
                        column: x => x.task_id,
                        principalTable: "service_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_service_files_uploader_account_id",
                        column: x => x.uploader_account_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_files_engagement_id_created_at",
                table: "service_files",
                columns: new[] { "engagement_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_service_files_object_name",
                table: "service_files",
                column: "object_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_files_task_id",
                table: "service_files",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_files_uploader_account_id_operation_id",
                table: "service_files",
                columns: new[] { "uploader_account_id", "operation_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_files");
        }
    }
}
