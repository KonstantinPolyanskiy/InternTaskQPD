using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Car.Dal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbArchForDetailsFeatureV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "photos");

            migrationBuilder.DropColumn(
                name: "CarType",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "CurrentOwner",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "Mileage",
                table: "cars");

            migrationBuilder.RenameColumn(
                name: "Extension",
                table: "photos",
                newName: "extension");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "photos",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhotoBytes",
                table: "photos",
                newName: "photo_bytes");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "cars",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "cars",
                newName: "color");

            migrationBuilder.RenameColumn(
                name: "Brand",
                table: "cars",
                newName: "brand");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cars",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhotoId",
                table: "cars",
                newName: "photo_id");

            migrationBuilder.RenameIndex(
                name: "IX_cars_PhotoId",
                table: "cars",
                newName: "IX_cars_photo_id");

            migrationBuilder.AlterColumn<string>(
                name: "extension",
                table: "photos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "cars",
                type: "integer",
                nullable: false,
                comment: "Primary key",
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "photo_id",
                table: "cars",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "car_condition",
                table: "cars",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Car condition");

            migrationBuilder.AddColumn<string>(
                name: "car_details",
                table: "cars",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority_sale",
                table: "cars",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Car priority sale");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "car_condition",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "car_details",
                table: "cars");

            migrationBuilder.DropColumn(
                name: "priority_sale",
                table: "cars");

            migrationBuilder.RenameColumn(
                name: "extension",
                table: "photos",
                newName: "Extension");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "photos",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "photo_bytes",
                table: "photos",
                newName: "PhotoBytes");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "cars",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "color",
                table: "cars",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "brand",
                table: "cars",
                newName: "Brand");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "cars",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "photo_id",
                table: "cars",
                newName: "PhotoId");

            migrationBuilder.RenameIndex(
                name: "IX_cars_photo_id",
                table: "cars",
                newName: "IX_cars_PhotoId");

            migrationBuilder.AlterColumn<string>(
                name: "Extension",
                table: "photos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "photos",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "cars",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Primary key")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PhotoId",
                table: "cars",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<byte>(
                name: "CarType",
                table: "cars",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentOwner",
                table: "cars",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mileage",
                table: "cars",
                type: "integer",
                nullable: true);
        }
    }
}
