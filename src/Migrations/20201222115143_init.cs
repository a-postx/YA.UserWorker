using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YA.TenantWorker.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiRequests",
                columns: table => new
                {
                    ApiRequestID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiRequestDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequests", x => x.ApiRequestID);
                });

            migrationBuilder.CreateTable(
                name: "PricingTiers",
                columns: table => new
                {
                    PricingTierID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HasTrial = table.Column<bool>(type: "bit", nullable: false),
                    TrialPeriod = table.Column<long>(type: "bigint", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxVkCommunities = table.Column<int>(type: "int", nullable: false),
                    MaxVkCommunitySize = table.Column<int>(type: "int", nullable: false),
                    MaxScheduledTasks = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.PricingTierID);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AuthProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PricingTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricingTierActivatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PricingTierActivatedUntilDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantID);
                    table.ForeignKey(
                        name: "FK_Tenants_PricingTiers_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierID");
                });

            migrationBuilder.CreateTable(
                name: "ClientInfos",
                columns: table => new
                {
                    YaClientInfoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ClientVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountryName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegionName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Os = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OsVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BrowserVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScreenResolution = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ViewportSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientInfos", x => x.YaClientInfoID);
                    table.ForeignKey(
                        name: "FK_ClientInfos_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPendingActivation = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "PricingTierID", "Description", "HasTrial", "MaxScheduledTasks", "MaxUsers", "MaxVkCommunities", "MaxVkCommunitySize", "Title", "TrialPeriod" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "Бесплатно для всех.", false, 0, 1, 1, 1000, "Бесплатный", null });

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "PricingTierID", "Description", "HasTrial", "MaxScheduledTasks", "MaxUsers", "MaxVkCommunities", "MaxVkCommunitySize", "Title", "TrialPeriod" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000013"), "За денежки", true, 1, 1, 1, 10000, "Платный", 12960000000000L });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "AuthProvider", "Email", "ExternalId", "IsReadOnly", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, null, null, false, "Системный", new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(737), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000001"), 1, 0 });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "AuthProvider", "Email", "ExternalId", "IsReadOnly", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), null, null, null, false, "Уважаемый", new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(2504), new DateTime(2021, 1, 21, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(2511), new Guid("00000000-0000-0000-0000-000000000013"), 1, 1 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedDateTime", "Email", "FirstName", "IsActive", "IsDeleted", "IsPendingActivation", "LastLoginDate", "LastModifiedDateTime", "LastName", "Password", "Role", "TenantId", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(3711), "admin@email.com", "My", true, false, false, null, new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(3716), "Admin", "123", "Administrator", new Guid("00000000-0000-0000-0000-000000000001"), "admin@ya.ru" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedDateTime", "Email", "FirstName", "IsActive", "IsDeleted", "IsPendingActivation", "LastLoginDate", "LastModifiedDateTime", "LastName", "Password", "Role", "TenantId", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000012"), new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(5640), "user@email.com", "My", true, false, false, null, new DateTime(2020, 12, 22, 11, 51, 42, 713, DateTimeKind.Utc).AddTicks(5646), "User", "123", "User", new Guid("00000000-0000-0000-0000-000000000001"), "user@ya.ru" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientInfos_TenantId",
                table: "ClientInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PricingTierId",
                table: "Tenants",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiRequests");

            migrationBuilder.DropTable(
                name: "ClientInfos");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "PricingTiers");
        }
    }
}
