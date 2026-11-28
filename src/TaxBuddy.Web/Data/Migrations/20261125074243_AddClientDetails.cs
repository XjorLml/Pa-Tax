using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // F17 (PRD R25): a client's phone, city and business. No code wrote Identity's phone_number before F17, so
            // limiting it to 20 characters loses nothing; PostgreSQL would refuse the change rather than cut a longer value.
            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "business_name",
                table: "users",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "business_type",
                table: "users",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "industry",
                table: "users",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "business_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "business_type",
                table: "users");

            migrationBuilder.DropColumn(
                name: "city",
                table: "users");

            migrationBuilder.DropColumn(
                name: "industry",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "phone_number",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
