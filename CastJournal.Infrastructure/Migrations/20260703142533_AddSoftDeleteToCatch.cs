using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Catches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Catches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Catches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Catches");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Catches");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Catches");
        }
    }
}
