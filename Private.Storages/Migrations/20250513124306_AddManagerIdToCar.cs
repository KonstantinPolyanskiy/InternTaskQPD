using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Private.Storages.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerIdToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "responsive_manager_id",
                table: "Cars",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Id менеджера ответственного за машину");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "responsive_manager_id",
                table: "Cars");
        }
    }
}
