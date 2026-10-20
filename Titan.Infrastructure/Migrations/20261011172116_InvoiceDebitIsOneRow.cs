using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Titan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDebitIsOneRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_status_created_at",
                schema: "core",
                table: "wallet_mutations");

            // Every existing row starts with queued_at = created_at, not now(): a pending row keeps its real age.
            // The now() default then serves the previous image, whose inserts do not name the column while the
            // rollout replaces it.
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "queued_at",
                schema: "core",
                table: "wallet_mutations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE core.wallet_mutations SET queued_at = created_at;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "queued_at",
                schema: "core",
                table: "wallet_mutations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            // Fails, and the whole migration rolls back, while a source still holds two rows (a failed invoice
            // debit beside its retry): each such pair needs a decision before this runs.
            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "ref_type", "ref_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_status_queued_at",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "status", "queued_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A reopened row loses only its queued_at; the filtered index still holds every row this one held.
            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_status_queued_at",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.DropColumn(
                name: "queued_at",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "ref_type", "ref_id" },
                unique: true,
                filter: "NOT (ref_type = 'Invoice' AND status = 'Failed')");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_status_created_at",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "status", "created_at" });
        }
    }
}
