using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxBuddy.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "owner_account_id",
                table: "accountant_profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "accountant_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_accountant_profiles_owner_account_id",
                table: "accountant_profiles",
                column: "owner_account_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_accountant_profiles_owner_account_id",
                table: "accountant_profiles",
                column: "owner_account_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_accountant_profiles_owner_account_id",
                table: "accountant_profiles");

            migrationBuilder.DropIndex(
                name: "ix_accountant_profiles_owner_account_id",
                table: "accountant_profiles");

            migrationBuilder.DropColumn(
                name: "owner_account_id",
                table: "accountant_profiles");

            migrationBuilder.DropColumn(
                name: "version",
                table: "accountant_profiles");
        }
    }
}
