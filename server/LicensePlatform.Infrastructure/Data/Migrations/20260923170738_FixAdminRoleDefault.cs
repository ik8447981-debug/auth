using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicensePlatform.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminRoleDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "AdminUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Viewer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "AdminUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Viewer",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
