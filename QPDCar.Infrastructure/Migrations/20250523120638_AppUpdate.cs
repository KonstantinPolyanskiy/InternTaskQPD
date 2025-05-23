using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPDCar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AppUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSold",
                table: "Cars",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSold",
                table: "Cars");
        }
    }
}
