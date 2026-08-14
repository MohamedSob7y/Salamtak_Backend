using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salamtak.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalReportAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalReportEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByType = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalReportAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalReportAttachments_MedicalReportEntries_MedicalReportEntryId",
                        column: x => x.MedicalReportEntryId,
                        principalTable: "MedicalReportEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalReportAttachments_MedicalReports_MedicalReportId",
                        column: x => x.MedicalReportId,
                        principalTable: "MedicalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalReportAttachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReportAttachments_MedicalReportEntryId",
                table: "MedicalReportAttachments",
                column: "MedicalReportEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReportAttachments_MedicalReportId",
                table: "MedicalReportAttachments",
                column: "MedicalReportId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalReportAttachments_UploadedByUserId",
                table: "MedicalReportAttachments",
                column: "UploadedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalReportAttachments");
        }
    }
}
