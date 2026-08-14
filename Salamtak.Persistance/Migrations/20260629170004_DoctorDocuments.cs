using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salamtak.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class DoctorDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "DoctorDocuments");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "DoctorDocuments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DoctorDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "DoctorDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "DoctorDocuments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "DoctorDocuments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "DoctorDocuments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorDocuments_DoctorId_DocumentType",
                table: "DoctorDocuments",
                columns: new[] { "DoctorId", "DocumentType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorDocuments_DoctorId_DocumentType",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "DoctorDocuments");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "DoctorDocuments");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "DoctorDocuments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
