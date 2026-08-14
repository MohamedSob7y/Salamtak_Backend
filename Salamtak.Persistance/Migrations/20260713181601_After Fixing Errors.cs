using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salamtak.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AfterFixingErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateTable(
                name: "DoctorClinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    DoctorId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    ClinicId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true),

                    IsDeleted = table.Column<bool>(
                        type: "bit",
                        nullable: false,
                        defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_DoctorClinics",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_DoctorClinics_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_DoctorClinics_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            /*
             * 2) نقل العلاقة القديمة:
             *
             * Clinics.DoctorId
             *          ↓
             * DoctorClinics.DoctorId + ClinicId
             *
             * نحافظ أيضًا على CreatedAt وUpdatedAt وIsDeleted.
             */
            migrationBuilder.Sql(
                @"
                INSERT INTO DoctorClinics
                (
                    Id,
                    DoctorId,
                    ClinicId,
                    CreatedAt,
                    UpdatedAt,
                    IsDeleted
                )
                SELECT
                    NEWID(),
                    DoctorId,
                    Id,
                    CreatedAt,
                    UpdatedAt,
                    IsDeleted
                FROM Clinics
                WHERE DoctorId IS NOT NULL;
                ");

            
            migrationBuilder.CreateIndex(
                name: "IX_DoctorClinics_ClinicId",
                table: "DoctorClinics",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorClinics_DoctorId_ClinicId",
                table: "DoctorClinics",
                columns: new[]
                {
                    "DoctorId",
                    "ClinicId"
                },
                unique: true);

           
            migrationBuilder.DropForeignKey(
                name: "FK_Clinics_Doctors_DoctorId",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_DoctorId",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Clinics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "Clinics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"
                IF EXISTS
                (
                    SELECT ClinicId
                    FROM DoctorClinics
                    GROUP BY ClinicId
                    HAVING COUNT(*) <> 1
                )
                OR EXISTS
                (
                    SELECT 1
                    FROM Clinics c
                    WHERE NOT EXISTS
                    (
                        SELECT 1
                        FROM DoctorClinics dc
                        WHERE dc.ClinicId = c.Id
                    )
                )
                BEGIN
                    THROW 51000,
                    'Cannot rollback: every clinic must have exactly one doctor relation.',
                    1;
                END;
                ");

            migrationBuilder.Sql(
                @"
                UPDATE c
                SET c.DoctorId = dc.DoctorId
                FROM Clinics c
                INNER JOIN DoctorClinics dc
                    ON dc.ClinicId = c.Id;
                ");

            migrationBuilder.AlterColumn<Guid>(
                name: "DoctorId",
                table: "Clinics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "DoctorClinics");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_DoctorId",
                table: "Clinics",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clinics_Doctors_DoctorId",
                table: "Clinics",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}