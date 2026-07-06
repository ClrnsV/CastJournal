using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CastJournal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCatchMediaFilterAndUserNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catches_FishingLocations_LocationId",
                table: "Catches");

            migrationBuilder.DropForeignKey(
                name: "FK_Catches_Species_SpeciesId",
                table: "Catches");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FishingLocations",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FishingLocations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "FishingLocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "FishingLocations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FishingLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Catches_FishingLocations_LocationId",
                table: "Catches",
                column: "LocationId",
                principalTable: "FishingLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Catches_Species_SpeciesId",
                table: "Catches",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Catches_FishingLocations_LocationId",
                table: "Catches");

            migrationBuilder.DropForeignKey(
                name: "FK_Catches_Species_SpeciesId",
                table: "Catches");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "FishingLocations");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "FishingLocations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FishingLocations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "FishingLocations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FishingLocations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Catches_FishingLocations_LocationId",
                table: "Catches",
                column: "LocationId",
                principalTable: "FishingLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Catches_Species_SpeciesId",
                table: "Catches",
                column: "SpeciesId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
