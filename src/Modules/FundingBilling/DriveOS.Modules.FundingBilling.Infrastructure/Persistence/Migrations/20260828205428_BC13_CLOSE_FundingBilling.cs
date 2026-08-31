using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BC13_CLOSE_FundingBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplier_invoices",
                schema: "funding_billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    ExternalSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceStatementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierReference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SettlementStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SettlementUpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OverdueAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MatchedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MatchedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationallyApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OperationallyApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinanciallyApprovedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinanciallyApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledForPaymentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduledForPaymentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_payment_attempts",
                schema: "funding_billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BankReference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsManual = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    SettledAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SettledOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ReconciliationDifference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ReconciliationStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ProcessingAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProviderReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_payment_attempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_payment_batches",
                schema: "funding_billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_payment_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_payment_refunds",
                schema: "funding_billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierInvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Method = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RefundedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_payment_refunds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_invoices_ClientOrganizationId_Status_DueDate",
                schema: "funding_billing",
                table: "supplier_invoices",
                columns: new[] { "ClientOrganizationId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_invoices_SourceType_ExternalSourceId",
                schema: "funding_billing",
                table: "supplier_invoices",
                columns: new[] { "SourceType", "ExternalSourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_invoices_SupplierOrganizationId_Status_DueDate",
                schema: "funding_billing",
                table: "supplier_invoices",
                columns: new[] { "SupplierOrganizationId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_invoices_SupplierReference_SupplierOrganizationId",
                schema: "funding_billing",
                table: "supplier_invoices",
                columns: new[] { "SupplierReference", "SupplierOrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_attempts_BatchId",
                schema: "funding_billing",
                table: "supplier_payment_attempts",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_attempts_ClientOrganizationId_Status_Sched~",
                schema: "funding_billing",
                table: "supplier_payment_attempts",
                columns: new[] { "ClientOrganizationId", "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_attempts_ProviderReference",
                schema: "funding_billing",
                table: "supplier_payment_attempts",
                column: "ProviderReference");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_attempts_SupplierInvoiceId_CreatedAtUtc",
                schema: "funding_billing",
                table: "supplier_payment_attempts",
                columns: new[] { "SupplierInvoiceId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_batches_OrganizationId_ScheduledDate_Status",
                schema: "funding_billing",
                table: "supplier_payment_batches",
                columns: new[] { "OrganizationId", "ScheduledDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_refunds_ProviderReference",
                schema: "funding_billing",
                table: "supplier_payment_refunds",
                column: "ProviderReference");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_payment_refunds_SupplierInvoiceId_RefundedAtUtc",
                schema: "funding_billing",
                table: "supplier_payment_refunds",
                columns: new[] { "SupplierInvoiceId", "RefundedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_invoices",
                schema: "funding_billing");

            migrationBuilder.DropTable(
                name: "supplier_payment_attempts",
                schema: "funding_billing");

            migrationBuilder.DropTable(
                name: "supplier_payment_batches",
                schema: "funding_billing");

            migrationBuilder.DropTable(
                name: "supplier_payment_refunds",
                schema: "funding_billing");
        }
    }
}
