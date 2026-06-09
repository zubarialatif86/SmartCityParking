using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCityParking.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToParkingSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ParkingSlots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "ParkingSlots",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
