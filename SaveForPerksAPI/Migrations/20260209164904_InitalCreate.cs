using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SaveForPerksAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "business_category",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
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
                    table.PrimaryKey("PK_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "business",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business", x => x.id);
                    table.ForeignKey(
                        name: "FK_business_business_category_category_id",
                        column: x => x.category_id,
                        principalTable: "business_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "business_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    auth_provider_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_admin = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_business_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_business_user_business_business_id",
                        column: x => x.business_id,
                        principalTable: "business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reward",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    business_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    cost_points = table.Column<int>(type: "int", nullable: false),
                    reward_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reward", x => x.id);
                    table.ForeignKey(
                        name: "FK_reward_business_business_id",
                        column: x => x.business_id,
                        principalTable: "business",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_balance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    balance = table.Column<int>(type: "int", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_balance", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_balance_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_customer_balance_reward_reward_id",
                        column: x => x.reward_id,
                        principalTable: "reward",
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
                    business_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    redeemed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reward_redemption", x => x.id);
                    table.ForeignKey(
                        name: "FK_reward_redemption_business_user_business_user_id",
                        column: x => x.business_user_id,
                        principalTable: "business_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reward_redemption_customer_user_id",
                        column: x => x.user_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reward_redemption_reward_reward_id",
                        column: x => x.reward_id,
                        principalTable: "reward",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "scan_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reward_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    business_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    qr_code_value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    scanned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    points_change = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scan_event", x => x.id);
                    table.ForeignKey(
                        name: "FK_scan_event_business_user_business_user_id",
                        column: x => x.business_user_id,
                        principalTable: "business_user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_scan_event_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_scan_event_reward_reward_id",
                        column: x => x.reward_id,
                        principalTable: "reward",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "business_category",
                columns: new[] { "id", "image_url", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "/images/categories/cafe.png", "Cafe" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "/images/categories/coffee-shop.png", "Coffee Shop" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "/images/categories/restaurant.png", "Restaurant" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "/images/categories/bakery.png", "Bakery" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "/images/categories/bar-pub.png", "Bar & Pub" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_business_category_id",
                table: "business",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_business_category_name",
                table: "business_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_business_user_auth_provider_id",
                table: "business_user",
                column: "auth_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_business_user_business_id",
                table: "business_user",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_auth_provider_id",
                table: "customer",
                column: "auth_provider_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_email",
                table: "customer",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_qr_code_value",
                table: "customer",
                column: "qr_code_value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_balance_customer_id_reward_id",
                table: "customer_balance",
                columns: new[] { "customer_id", "reward_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_balance_reward_id",
                table: "customer_balance",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_business_id",
                table: "reward",
                column: "business_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemption_business_user_id",
                table: "reward_redemption",
                column: "business_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemption_reward_id",
                table: "reward_redemption",
                column: "reward_id");

            migrationBuilder.CreateIndex(
                name: "IX_reward_redemption_user_id",
                table: "reward_redemption",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scan_event_business_user_id",
                table: "scan_event",
                column: "business_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_scan_event_customer_id",
                table: "scan_event",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_scan_event_reward_id",
                table: "scan_event",
                column: "reward_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_balance");

            migrationBuilder.DropTable(
                name: "reward_redemption");

            migrationBuilder.DropTable(
                name: "scan_event");

            migrationBuilder.DropTable(
                name: "business_user");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "reward");

            migrationBuilder.DropTable(
                name: "business");

            migrationBuilder.DropTable(
                name: "business_category");
        }
    }
}
