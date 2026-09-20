using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LicensePlatform.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Initial migration that creates all tables for the License Platform.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Creates all database tables, indexes, and constraints.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Products
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    CurrentVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    MinimumSupportedVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    LatestVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            // AdminUsers
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Viewer"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.AdminUserId);
                });

            // SystemSettings
            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    SettingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "string"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.SettingId);
                });

            // SigningKeys
            migrationBuilder.CreateTable(
                name: "SigningKeys",
                columns: table => new
                {
                    KeyId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    KeyVersion = table.Column<int>(type: "integer", nullable: false),
                    PrivateKeyPem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PublicKeyPem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SigningKeys", x => x.KeyId);
                });

            // ApiClients
            migrationBuilder.CreateTable(
                name: "ApiClients",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SecretHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AllowedScopes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RateLimitOverride = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClients", x => x.ClientId);
                });

            // ProductFeatures
            migrationBuilder.CreateTable(
                name: "ProductFeatures",
                columns: table => new
                {
                    FeatureId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FeatureName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFeatures", x => x.FeatureId);
                    table.ForeignKey(
                        name: "FK_ProductFeatures_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ProductVersions
            migrationBuilder.CreateTable(
                name: "ProductVersions",
                columns: table => new
                {
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVersions", x => x.VersionId);
                    table.ForeignKey(
                        name: "FK_ProductVersions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // LicensePlans
            migrationBuilder.CreateTable(
                name: "LicensePlans",
                columns: table => new
                {
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    MaxDevices = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    OfflineGraceHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 72),
                    ValidationIntervalMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicensePlans", x => x.PlanId);
                    table.ForeignKey(
                        name: "FK_LicensePlans_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Customers
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, defaultValue: ""),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            // PlanFeatures
            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_LicensePlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "LicensePlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_ProductFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "ProductFeatures",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Licenses
            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    LicenseKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Created"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxDevices = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedValidationCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.LicenseId);
                    table.ForeignKey(
                        name: "FK_Licenses_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Licenses_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Licenses_LicensePlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "LicensePlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                });

            // AuditLogs
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValue: ""),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogId);
                });

            // LicenseFeatures
            migrationBuilder.CreateTable(
                name: "LicenseFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseFeatures_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "LicenseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LicenseFeatures_ProductFeatures_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "ProductFeatures",
                        principalColumn: "FeatureId",
                        onDelete: ReferentialAction.Cascade);
                });

            // LicenseDevices
            migrationBuilder.CreateTable(
                name: "LicenseDevices",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: ""),
                    OSVersion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseDevices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_LicenseDevices_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "LicenseId",
                        onDelete: ReferentialAction.Cascade);
                });

            // ValidationEvents
            migrationBuilder.CreateTable(
                name: "ValidationEvents",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false,
                        defaultValueSql: "gen_random_uuid()"),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceFingerprint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false, defaultValue: ""),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ApplicationVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_ValidationEvents_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "LicenseId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Unique indexes
            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeatures_ProductId_FeatureKey",
                table: "ProductFeatures",
                columns: new[] { "ProductId", "FeatureKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVersions_ProductId_VersionNumber",
                table: "ProductVersions",
                columns: new[] { "ProductId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlans_ProductId_PlanName",
                table: "LicensePlans",
                columns: new[] { "ProductId", "PlanName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId_FeatureId",
                table: "PlanFeatures",
                columns: new[] { "PlanId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ProductId_Email",
                table: "Customers",
                columns: new[] { "ProductId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseKey",
                table: "Licenses",
                column: "LicenseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ProductId",
                table: "Licenses",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_CustomerId",
                table: "Licenses",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Status",
                table: "Licenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ExpiryDate",
                table: "Licenses",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseFeatures_LicenseId_FeatureId",
                table: "LicenseFeatures",
                columns: new[] { "LicenseId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevices_LicenseId_DeviceFingerprint",
                table: "LicenseDevices",
                columns: new[] { "LicenseId", "DeviceFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseDevices_DeviceFingerprint",
                table: "LicenseDevices",
                column: "DeviceFingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationEvents_LicenseId",
                table: "ValidationEvents",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationEvents_Timestamp",
                table: "ValidationEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationEvents_EventType",
                table: "ValidationEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Username",
                table: "AdminUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorName",
                table: "AuditLogs",
                column: "ActorName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TargetType_TargetId",
                table: "AuditLogs",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_KeyVersion",
                table: "SigningKeys",
                column: "KeyVersion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_IsActive",
                table: "SigningKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_SettingKey",
                table: "SystemSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiClients_ApiKey",
                table: "ApiClients",
                column: "ApiKey",
                unique: true);
        }

        /// <summary>
        /// Drops all database tables in reverse order.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ValidationEvents");
            migrationBuilder.DropTable(name: "LicenseDevices");
            migrationBuilder.DropTable(name: "LicenseFeatures");
            migrationBuilder.DropTable(name: "PlanFeatures");
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "Licenses");
            migrationBuilder.DropTable(name: "Customers");
            migrationBuilder.DropTable(name: "LicensePlans");
            migrationBuilder.DropTable(name: "ProductFeatures");
            migrationBuilder.DropTable(name: "ProductVersions");
            migrationBuilder.DropTable(name: "Products");
            migrationBuilder.DropTable(name: "AdminUsers");
            migrationBuilder.DropTable(name: "SigningKeys");
            migrationBuilder.DropTable(name: "SystemSettings");
            migrationBuilder.DropTable(name: "ApiClients");
        }
    }
}
