using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateFormKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "form_key",
                table: "service_updates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_updates_engagement_id_form_key",
                table: "service_updates",
                columns: new[] { "engagement_id", "form_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_service_updates_engagement_id_form_key",
                table: "service_updates");

            migrationBuilder.DropColumn(
                name: "form_key",
                table: "service_updates");
        }
    }
}
