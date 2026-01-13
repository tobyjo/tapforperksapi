using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TapForPerksAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reward_owner",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reward___3213E83FEC734646", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    auth_provider_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    qr_code_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F4461F44E", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reward",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    reward_owner_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    cost_points = table.Column<int>(type: "int", nullable: true),
                    reward_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reward___3213E83F9D47D4D7", x => x.id);
                    table.ForeignKey(
                        name: "fk_reward_owner",
                        column: x => x.reward_owner_id,
                        principalTable: "reward_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reward_owner_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    reward_owner_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    auth_provider_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_admin = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reward___3213E83FCA9F25E5", x => x.id);
                    table.ForeignKey(
                        name: "fk_reward_owner_user_owner",
                        column: x => x.reward_owner_id,
                        principalTable: "reward_owner",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_balance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    balance = table.Column<int>(type: "int", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_bal__3213E83F23104F90", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_balance_programme",
                        column: x => x.reward_id,
                        principalTable: "reward",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_balance_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reward_redemption",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_owner_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    redeemed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reward_r__3213E83FC9ADA235", x => x.id);
                    table.ForeignKey(
                        name: "fk_reward_redemption_owner_user",
                        column: x => x.reward_owner_user_id,
                        principalTable: "reward_owner_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reward_redemption_programme",
                        column: x => x.reward_id,
                        principalTable: "reward",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reward_redemption_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scan_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_owner_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    qr_code_value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    scanned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    points_change = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__scan_eve__3213E83F45201827", x => x.id);
                    table.ForeignKey(
                        name: "fk_scan_event_owner_user",
                        column: x => x.reward_owner_user_id,
                        principalTable: "reward_owner_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scan_event_programme",
                        column: x => x.reward_id,
                        principalTable: "reward",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_scan_event_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "reward_owner",
                columns: new[] { "id", "address", "created_at", "Description", "metadata", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "123 High Street, London, UK", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium artisan coffee shop chain", null, "The Daily Grind Coffee" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "456 Market Square, Manchester, UK", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Private event", null, "Smith-Jones Wedding" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "auth_provider_id", "created_at", "email", "name", "qr_code_value" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-999999999999"), "auth0|user001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "alice@example.com", "Alice Customer", "QR001-ALICE-9999" },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "auth0|user002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "bob@example.com", "Bob Customer", "QR002-BOB-AAAA" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "auth0|user003", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "charlie@example.com", "Charlie Customer", "QR003-CHARLIE-BBBB" }
                });

            migrationBuilder.InsertData(
                table: "reward",
                columns: new[] { "id", "cost_points", "created_at", "is_active", "metadata", "name", "reward_owner_id", "reward_type" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Free Coffee at 5 points", new Guid("11111111-1111-1111-1111-111111111111"), "incremental_points" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "Wedding Drink Allowance of 2 drinks", new Guid("22222222-2222-2222-2222-222222222222"), "allowance_limit" }
                });

            migrationBuilder.InsertData(
                table: "reward_owner_user",
                columns: new[] { "id", "auth_provider_id", "created_at", "email", "is_admin", "name", "reward_owner_id" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "auth0|admin001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "baristaone@dailygrind.com", true, "Barista One", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), "auth0|admin002", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "host@wedding.com", true, "Wedding Host", new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.CreateIndex(
                name: "idx_reward_owner_id",
                table: "reward",
                column: "reward_owner_id");

            migrationBuilder.CreateIndex(
                name: "idx_reward_owner_user_owner_id",
                table: "reward_owner_user",
                column: "reward_owner_id");

            migrationBuilder.CreateIndex(
                name: "idx_reward_redemption_reward_id",
                table: "reward_redemption",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "idx_reward_redemption_user_id",
                table: "reward_redemption",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemption_reward_owner_user_id",
                table: "reward_redemption",
                column: "reward_owner_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_scan_event_programme_id",
                table: "scan_event",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "idx_scan_event_user_id",
                table: "scan_event",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scan_event_reward_owner_user_id",
                table: "scan_event",
                column: "reward_owner_user_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_balance_programme_id",
                table: "user_balance",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "idx_user_balance_user_id",
                table: "user_balance",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_user_balance",
                table: "user_balance",
                columns: new[] { "user_id", "reward_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E6164CB0A1AE0",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__users__C82CBBE99CDF45A3",
                table: "users",
                column: "auth_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__users__C8EB4B8153934E5A",
                table: "users",
                column: "qr_code_value",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reward_redemption");

            migrationBuilder.DropTable(
                name: "scan_event");

            migrationBuilder.DropTable(
                name: "user_balance");

            migrationBuilder.DropTable(
                name: "reward_owner_user");

            migrationBuilder.DropTable(
                name: "reward");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "reward_owner");
        }
    }
}
