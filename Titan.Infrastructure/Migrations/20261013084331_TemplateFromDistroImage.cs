using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Titan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TemplateFromDistroImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "image_url",
                schema: "core",
                table: "vm_templates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_listed",
                schema: "core",
                table: "vm_templates",
                type: "boolean",
                nullable: false,
                // Edited from the scaffold's false: a template row outside the seed stays offered as it was. The
                // three seed rows get explicit values below. The model keeps no store default (VmTemplateConfiguration).
                defaultValue: true);

            migrationBuilder.UpdateData(
                schema: "core",
                table: "vm_templates",
                keyColumn: "id",
                keyValue: new Guid("0a5e0000-0000-4000-8000-000000000002"),
                columns: new[] { "image_url", "is_listed" },
                values: new object[] { "https://cloud-images.ubuntu.com/releases/jammy/release/ubuntu-22.04-server-cloudimg-{arch}.img", true });

            migrationBuilder.UpdateData(
                schema: "core",
                table: "vm_templates",
                keyColumn: "id",
                keyValue: new Guid("0a5e0000-0000-4000-8000-000000000003"),
                columns: new[] { "image_url", "is_listed" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                schema: "core",
                table: "vm_templates",
                keyColumn: "id",
                keyValue: new Guid("0a5e0000-0000-4000-8000-000000000005"),
                columns: new[] { "image_url", "is_listed" },
                values: new object[] { null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_url",
                schema: "core",
                table: "vm_templates");

            migrationBuilder.DropColumn(
                name: "is_listed",
                schema: "core",
                table: "vm_templates");
        }
    }
}
