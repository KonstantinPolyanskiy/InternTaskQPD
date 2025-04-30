using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Dal.Migrations.AuthDb
{
    /// <inheritdoc />
    public partial class AddJtiPropertyToRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "jti",
                table: "refresh_tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "jti access токена, выданный вместе с refresh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jti",
                table: "refresh_tokens");
        }
    }
}
