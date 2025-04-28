using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageTypePropertyToCarTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StorageType",
                table: "cars",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "cars");
        }
    }
}
