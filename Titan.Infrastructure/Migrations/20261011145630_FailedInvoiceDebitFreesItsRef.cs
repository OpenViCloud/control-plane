using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Titan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FailedInvoiceDebitFreesItsRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "ref_type", "ref_id" },
                unique: true,
                filter: "NOT (ref_type = 'Invoice' AND status = 'Failed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Refused by Postgres once an invoice holds a failed debit AND a later one: the unfiltered
            // index cannot cover both. Delete the failed invoice debits first if this must roll back.
            migrationBuilder.DropIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "ref_type", "ref_id" },
                unique: true);
        }
    }
}
