using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Titan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResourcesReferenceTheirOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_volumes_volume_offering_id",
                schema: "core",
                table: "volumes",
                column: "volume_offering_id");

            migrationBuilder.CreateIndex(
                name: "IX_vms_vm_offering_id",
                schema: "core",
                table: "vms",
                column: "vm_offering_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vms_vm_offerings_vm_offering_id",
                schema: "core",
                table: "vms",
                column: "vm_offering_id",
                principalSchema: "core",
                principalTable: "vm_offerings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_volumes_volume_offerings_volume_offering_id",
                schema: "core",
                table: "volumes",
                column: "volume_offering_id",
                principalSchema: "core",
                principalTable: "volume_offerings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vms_vm_offerings_vm_offering_id",
                schema: "core",
                table: "vms");

            migrationBuilder.DropForeignKey(
                name: "FK_volumes_volume_offerings_volume_offering_id",
                schema: "core",
                table: "volumes");

            migrationBuilder.DropIndex(
                name: "IX_volumes_volume_offering_id",
                schema: "core",
                table: "volumes");

            migrationBuilder.DropIndex(
                name: "IX_vms_vm_offering_id",
                schema: "core",
                table: "vms");
        }
    }
}
