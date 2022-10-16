using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace elefanti.video.backend.Migrations
{
    public partial class categoryimageupload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "Videos");

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Categories");

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "Videos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
