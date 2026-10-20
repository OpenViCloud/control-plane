using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Titan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "activity_events",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    actor_keycloak_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_audit_log",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    actor_keycloak_user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    resource_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resource_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    detail = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clusters",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    zone_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    endpoint = table.Column<string>(type: "text", nullable: false),
                    server_fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ovn_uplink_network = table.Column<string>(type: "text", nullable: true),
                    storage_pool = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clusters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "firewall_rules",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    protocol = table.Column<string>(type: "text", nullable: false),
                    start_port = table.Column<int>(type: "integer", nullable: true),
                    end_port = table.Column<int>(type: "integer", nullable: true),
                    cidr = table.Column<string>(type: "text", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    current_state = table.Column<string>(type: "text", nullable: true),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firewall_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    response_status_code = table.Column<int>(type: "integer", nullable: true),
                    response_body = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_year = table.Column<int>(type: "integer", nullable: false),
                    period_month = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    paid_by = table.Column<string>(type: "text", nullable: true),
                    total = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "legal_acceptances",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<string>(type: "text", nullable: false),
                    document_key = table.Column<string>(type: "text", nullable: false),
                    document_version = table.Column<string>(type: "text", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    source_ip = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legal_acceptances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "networks",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    zone_id = table.Column<string>(type: "text", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    cidr = table.Column<string>(type: "text", nullable: true),
                    gateway = table.Column<string>(type: "text", nullable: true),
                    current_state = table.Column<string>(type: "text", nullable: true),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    default_ingress_policy = table.Column<string>(type: "text", nullable: false, defaultValue: "Allow"),
                    default_egress_policy = table.Column<string>(type: "text", nullable: false, defaultValue: "Allow")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_networks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    top_up_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    gateway = table.Column<string>(type: "text", nullable: false),
                    raw_content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_client_identities",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    certificate_pem = table.Column<string>(type: "text", nullable: false),
                    key_cipher = table.Column<byte[]>(type: "bytea", nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_client_identities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provider_commands",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    zone_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    rerun = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    available_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lease_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lease_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_commands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "snapshot_offerings",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    price_per_hour = table.Column<decimal>(type: "numeric(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snapshot_offerings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    debt_ceiling = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    max_firewall_rules_per_network = table.Column<int>(type: "integer", nullable: false),
                    max_networks = table.Column<int>(type: "integer", nullable: false),
                    max_public_ipv6 = table.Column<int>(type: "integer", nullable: false),
                    max_vm_snapshots_per_vm = table.Column<int>(type: "integer", nullable: false),
                    max_vms = table.Column<int>(type: "integer", nullable: false),
                    max_volume_snapshots_per_volume = table.Column<int>(type: "integer", nullable: false),
                    max_volumes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "top_ups",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    payment_ref = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true),
                    payment_txn_ref = table.Column<string>(type: "text", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    confirmed_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_top_ups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usage_records",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "text", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bucket = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    interval_minutes = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    usage_type = table.Column<string>(type: "text", nullable: false),
                    unit_price_per_hour = table.Column<decimal>(type: "numeric(18,10)", nullable: false),
                    cost = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    collected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vm_offerings",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    cpu_units = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    vcpu_count = table.Column<int>(type: "integer", nullable: false),
                    memory_mb = table.Column<int>(type: "integer", nullable: false),
                    price_per_hour = table.Column<decimal>(type: "numeric(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vm_offerings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vm_snapshots",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vm_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vms",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    current_state = table.Column<string>(type: "text", nullable: true),
                    desired_state = table.Column<string>(type: "text", nullable: false),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    has_been_running = table.Column<bool>(type: "boolean", nullable: false),
                    create_finished = table.Column<bool>(type: "boolean", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    enable_public_ipv6 = table.Column<bool>(type: "boolean", nullable: false),
                    public_ipv6_address = table.Column<string>(type: "text", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    vm_offering_id = table.Column<string>(type: "text", nullable: false),
                    template_id = table.Column<string>(type: "text", nullable: false),
                    zone_id = table.Column<string>(type: "text", nullable: false),
                    disk_size_gb = table.Column<int>(type: "integer", nullable: false),
                    user_data = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "volume_offerings",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    size_gb = table.Column<int>(type: "integer", nullable: false),
                    is_customized = table.Column<bool>(type: "boolean", nullable: false),
                    price_per_gb_per_hour = table.Column<decimal>(type: "numeric(18,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volume_offerings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "volume_snapshots",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    volume_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volume_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "volumes",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zone_id = table.Column<string>(type: "text", nullable: false),
                    volume_offering_id = table.Column<string>(type: "text", nullable: false),
                    size_gb = table.Column<int>(type: "integer", nullable: false),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    current_attached_vm_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_attached_elsewhere = table.Column<bool>(type: "boolean", nullable: false),
                    desired_attached_vm_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volumes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_mutations",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    ref_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    applied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_mutations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    ref_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance_after = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
                    available_amount = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "text", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "core",
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_members",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    keycloak_user_id = table.Column<string>(type: "text", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_members_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "core",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vm_templates",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    os_image_name = table.Column<string>(type: "text", nullable: false),
                    root_disk_size_gb = table.Column<int>(type: "integer", nullable: true),
                    vm_offering_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vm_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_vm_templates_vm_offerings_vm_offering_id",
                        column: x => x.vm_offering_id,
                        principalSchema: "core",
                        principalTable: "vm_offerings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vm_template_zones",
                schema: "core",
                columns: table => new
                {
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zone_id = table.Column<string>(type: "text", nullable: false),
                    current_existence = table.Column<string>(type: "text", nullable: false),
                    desired_existence = table.Column<string>(type: "text", nullable: false),
                    pending_intent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_template_id = table.Column<string>(type: "text", nullable: true),
                    request_failure_reason = table.Column<string>(type: "text", nullable: true),
                    request_failure_detail = table.Column<string>(type: "text", nullable: true),
                    request_failed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vm_template_zones", x => new { x.template_id, x.zone_id });
                    table.ForeignKey(
                        name: "FK_vm_template_zones_vm_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "core",
                        principalTable: "vm_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "snapshot_offerings",
                columns: new[] { "id", "name", "price_per_hour" },
                values: new object[] { "standard", "Standard", 1.8m });

            migrationBuilder.InsertData(
                schema: "core",
                table: "vm_offerings",
                columns: new[] { "id", "cpu_units", "memory_mb", "name", "price_per_hour", "vcpu_count" },
                values: new object[,]
                {
                    { "l", 2.0m, 8192, "Large", 634.050m, 4 },
                    { "m", 1.0m, 4096, "Medium", 311.256m, 2 },
                    { "s", 0.5m, 2048, "Small", 161.388m, 1 },
                    { "xl", 4.0m, 16384, "Extra Large", 1268.100m, 8 }
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "volume_offerings",
                columns: new[] { "id", "is_customized", "name", "price_per_gb_per_hour", "size_gb" },
                values: new object[] { "standard", true, "Standard", 1.57914m, 0 });

            migrationBuilder.InsertData(
                schema: "core",
                table: "vm_templates",
                columns: new[] { "id", "created_at", "description", "name", "os_image_name", "root_disk_size_gb", "updated_at", "vm_offering_id" },
                values: new object[,]
                {
                    { new Guid("0a5e0000-0000-4000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ubuntu 22.04 LTS general purpose", "Ubuntu-22.04-Standard", "ubuntu/22.04", 25, new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "m" },
                    { new Guid("0a5e0000-0000-4000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "AlmaLinux 9 for database servers", "Alma-9-DB", "almalinux/9", 40, new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "l" },
                    { new Guid("0a5e0000-0000-4000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Rocky Linux 9 application server", "Rocky-9-App", "rockylinux/9", 30, new DateTimeOffset(new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "l" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_events_tenant_id_created_at",
                schema: "core",
                table: "activity_events",
                columns: new[] { "tenant_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_created_at",
                schema: "core",
                table: "admin_audit_log",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_admin_audit_log_owner_tenant_id",
                schema: "core",
                table: "admin_audit_log",
                column: "owner_tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_clusters_zone_id",
                schema: "core",
                table: "clusters",
                column: "zone_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_firewall_rules_network_id",
                schema: "core",
                table: "firewall_rules",
                column: "network_id");

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_created_at",
                schema: "core",
                table: "idempotency_keys",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_keys_tenant_id_key",
                schema: "core",
                table: "idempotency_keys",
                columns: new[] { "tenant_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_invoice_id",
                schema: "core",
                table: "invoice_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_tenant_id_period_year_period_month",
                schema: "core",
                table: "invoices",
                columns: new[] { "tenant_id", "period_year", "period_month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_legal_acceptances_subject_id_document_key_document_version",
                schema: "core",
                table: "legal_acceptances",
                columns: new[] { "subject_id", "document_key", "document_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_networks_tenant_id_name",
                schema: "core",
                table: "networks",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "current_existence <> 'Deleted'");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_status",
                schema: "core",
                table: "payment_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_top_up_id",
                schema: "core",
                table: "payment_transactions",
                column: "top_up_id");

            migrationBuilder.CreateIndex(
                name: "IX_provider_commands_key",
                schema: "core",
                table: "provider_commands",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_commands_lease_id",
                schema: "core",
                table: "provider_commands",
                column: "lease_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_provider_commands_status_kind_available_at",
                schema: "core",
                table: "provider_commands",
                columns: new[] { "status", "kind", "available_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_members_keycloak_user_id",
                schema: "core",
                table: "tenant_members",
                column: "keycloak_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_members_tenant_id_keycloak_user_id",
                schema: "core",
                table: "tenant_members",
                columns: new[] { "tenant_id", "keycloak_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_name",
                schema: "core",
                table: "tenants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_top_ups_payment_ref",
                schema: "core",
                table: "top_ups",
                column: "payment_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_top_ups_tenant_id",
                schema: "core",
                table: "top_ups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_resource_type_resource_id_bucket",
                schema: "core",
                table: "usage_records",
                columns: new[] { "resource_type", "resource_id", "bucket" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usage_records_tenant_id_bucket",
                schema: "core",
                table: "usage_records",
                columns: new[] { "tenant_id", "bucket" });

            migrationBuilder.CreateIndex(
                name: "IX_vm_snapshots_tenant_id",
                schema: "core",
                table: "vm_snapshots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_vm_snapshots_vm_id",
                schema: "core",
                table: "vm_snapshots",
                column: "vm_id");

            migrationBuilder.CreateIndex(
                name: "IX_vm_templates_vm_offering_id",
                schema: "core",
                table: "vm_templates",
                column: "vm_offering_id");

            migrationBuilder.CreateIndex(
                name: "UX_vm_templates_name",
                schema: "core",
                table: "vm_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vms_tenant_id_name",
                schema: "core",
                table: "vms",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "current_existence <> 'Deleted'");

            migrationBuilder.CreateIndex(
                name: "IX_volume_snapshots_tenant_id",
                schema: "core",
                table: "volume_snapshots",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_volume_snapshots_volume_id",
                schema: "core",
                table: "volume_snapshots",
                column: "volume_id");

            migrationBuilder.CreateIndex(
                name: "IX_volumes_tenant_id",
                schema: "core",
                table: "volumes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_ref_type_ref_id",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "ref_type", "ref_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_status_created_at",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_mutations_wallet_id_status",
                schema: "core",
                table: "wallet_mutations",
                columns: new[] { "wallet_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_type_ref_id",
                schema: "core",
                table: "wallet_transactions",
                columns: new[] { "type", "ref_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_wallet_id_created_at",
                schema: "core",
                table: "wallet_transactions",
                columns: new[] { "wallet_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_wallets_tenant_id",
                schema: "core",
                table: "wallets",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_events",
                schema: "core");

            migrationBuilder.DropTable(
                name: "admin_audit_log",
                schema: "core");

            migrationBuilder.DropTable(
                name: "clusters",
                schema: "core");

            migrationBuilder.DropTable(
                name: "firewall_rules",
                schema: "core");

            migrationBuilder.DropTable(
                name: "idempotency_keys",
                schema: "core");

            migrationBuilder.DropTable(
                name: "invoice_items",
                schema: "core");

            migrationBuilder.DropTable(
                name: "legal_acceptances",
                schema: "core");

            migrationBuilder.DropTable(
                name: "networks",
                schema: "core");

            migrationBuilder.DropTable(
                name: "payment_transactions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "provider_client_identities",
                schema: "core");

            migrationBuilder.DropTable(
                name: "provider_commands",
                schema: "core");

            migrationBuilder.DropTable(
                name: "snapshot_offerings",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tenant_members",
                schema: "core");

            migrationBuilder.DropTable(
                name: "top_ups",
                schema: "core");

            migrationBuilder.DropTable(
                name: "usage_records",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vm_snapshots",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vm_template_zones",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vms",
                schema: "core");

            migrationBuilder.DropTable(
                name: "volume_offerings",
                schema: "core");

            migrationBuilder.DropTable(
                name: "volume_snapshots",
                schema: "core");

            migrationBuilder.DropTable(
                name: "volumes",
                schema: "core");

            migrationBuilder.DropTable(
                name: "wallet_mutations",
                schema: "core");

            migrationBuilder.DropTable(
                name: "wallet_transactions",
                schema: "core");

            migrationBuilder.DropTable(
                name: "wallets",
                schema: "core");

            migrationBuilder.DropTable(
                name: "invoices",
                schema: "core");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vm_templates",
                schema: "core");

            migrationBuilder.DropTable(
                name: "vm_offerings",
                schema: "core");
        }
    }
}
