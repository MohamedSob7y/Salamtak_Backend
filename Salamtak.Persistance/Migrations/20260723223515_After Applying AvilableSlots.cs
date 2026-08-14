using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salamtak.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AfterApplyingAvilableSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [Status] IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AvailabilitySlotId",
                table: "Appointments",
                column: "AvailabilitySlotId",
                unique: true);
        }
    }
}
