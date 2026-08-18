using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveOS.Modules.Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contracts");

            migrationBuilder.CreateTable(
                name: "contract_amendments",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmendmentNumber = table.Column<int>(type: "integer", nullable: false),
                    BaseContractVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SignedDocumentReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SignedDocumentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SignatureRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AppliedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    terms_snapshot = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_amendments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contract_audit_entries",
                schema: "contracts",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    details_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_audit_entries", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "contract_documents",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    RetainUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    RetentionLegalBasis = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ArchivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "signature_processes",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    DocumentReference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DocumentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignatureOrder = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signature_processes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "training_contracts",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOfferVersion = table.Column<int>(type: "integer", nullable: false),
                    ContractNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedDocumentReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GeneratedDocumentFileName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    GeneratedDocumentContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GeneratedDocumentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    GeneratedDocumentVersionNumber = table.Column<int>(type: "integer", nullable: true),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuspensionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SuspensionEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SuspensionExpectedResumeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SuspendedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TerminationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TerminationEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TerminatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompletionEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpirationEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ExpiredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    terms_snapshot = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_contracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contract_document_versions",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    StorageReference = table.Column<string>(type: "character varying(700)", maxLength: 700, nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_document_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contract_document_versions_contract_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "contracts",
                        principalTable: "contract_documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "signature_evidence",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignatureProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignatoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SignatureMethod = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    AuthenticationMethod = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Provider = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ProviderSignatureReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CertificateReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signature_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_signature_evidence_signature_processes_SignatureProcessId",
                        column: x => x.SignatureProcessId,
                        principalSchema: "contracts",
                        principalTable: "signature_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "signature_process_recipients",
                schema: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    SignatoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepresentedOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SigningOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    signature_process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_signature_process_recipients", x => x.id);
                    table.ForeignKey(
                        name: "FK_signature_process_recipients_signature_processes_signature_~",
                        column: x => x.signature_process_id,
                        principalSchema: "contracts",
                        principalTable: "signature_processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_contract_parties",
                schema: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LegalReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_contract_parties", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_contract_parties_training_contracts_contract_id",
                        column: x => x.contract_id,
                        principalSchema: "contracts",
                        principalTable: "training_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_contract_signatories",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepresentedOrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SigningOrder = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorityReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AuthorityStatus = table.Column<int>(type: "integer", nullable: false),
                    AuthorityVerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorityVerifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AuthorityRejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_contract_signatories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_contract_signatories_training_contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contracts",
                        principalTable: "training_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_contract_versions",
                schema: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    SourceOfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceOfferVersion = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RevisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    terms_snapshot = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_contract_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_training_contract_versions_training_contracts_ContractId",
                        column: x => x.ContractId,
                        principalSchema: "contracts",
                        principalTable: "training_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "training_contract_version_parties",
                schema: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LegalReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    contract_version_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_contract_version_parties", x => x.id);
                    table.ForeignKey(
                        name: "FK_training_contract_version_parties_training_contract_version~",
                        column: x => x.contract_version_id,
                        principalSchema: "contracts",
                        principalTable: "training_contract_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contract_amendments_OrganizationId_ContractId_AmendmentNumb~",
                schema: "contracts",
                table: "contract_amendments",
                columns: new[] { "OrganizationId", "ContractId", "AmendmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contract_audit_entries_organization_id_action_occurred_at_u~",
                schema: "contracts",
                table: "contract_audit_entries",
                columns: new[] { "organization_id", "action", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_contract_audit_entries_organization_id_contract_id_occurred~",
                schema: "contracts",
                table: "contract_audit_entries",
                columns: new[] { "organization_id", "contract_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_contract_document_versions_DocumentId_VersionNumber",
                schema: "contracts",
                table: "contract_document_versions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contract_documents_OrganizationId_ContractId",
                schema: "contracts",
                table: "contract_documents",
                columns: new[] { "OrganizationId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_signature_evidence_Provider_ProviderSignatureReference",
                schema: "contracts",
                table: "signature_evidence",
                columns: new[] { "Provider", "ProviderSignatureReference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_signature_evidence_SignatoryId",
                schema: "contracts",
                table: "signature_evidence",
                column: "SignatoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_signature_evidence_SignatureProcessId",
                schema: "contracts",
                table: "signature_evidence",
                column: "SignatureProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_signature_process_recipients_signature_process_id",
                schema: "contracts",
                table: "signature_process_recipients",
                column: "signature_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_signature_processes_OrganizationId_ContractId_ContractVersi~",
                schema: "contracts",
                table: "signature_processes",
                columns: new[] { "OrganizationId", "ContractId", "ContractVersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_contract_parties_contract_id",
                schema: "contracts",
                table: "training_contract_parties",
                column: "contract_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_contract_signatories_ContractId_PersonId_Kind",
                schema: "contracts",
                table: "training_contract_signatories",
                columns: new[] { "ContractId", "PersonId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_contract_signatories_ContractId_SigningOrder",
                schema: "contracts",
                table: "training_contract_signatories",
                columns: new[] { "ContractId", "SigningOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_training_contract_version_parties_contract_version_id",
                schema: "contracts",
                table: "training_contract_version_parties",
                column: "contract_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_training_contract_versions_ContractId_VersionNumber",
                schema: "contracts",
                table: "training_contract_versions",
                columns: new[] { "ContractId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_training_contracts_OrganizationId_ContractNumber",
                schema: "contracts",
                table: "training_contracts",
                columns: new[] { "OrganizationId", "ContractNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_amendments",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "contract_audit_entries",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "contract_document_versions",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "signature_evidence",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "signature_process_recipients",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "training_contract_parties",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "training_contract_signatories",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "training_contract_version_parties",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "contract_documents",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "signature_processes",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "training_contract_versions",
                schema: "contracts");

            migrationBuilder.DropTable(
                name: "training_contracts",
                schema: "contracts");
        }
    }
}
