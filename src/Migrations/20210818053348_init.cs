using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YA.UserWorker.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientInfos",
                columns: table => new
                {
                    YaClientInfoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    MaxVkPeriodicParsingTasks = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.PricingTierID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AuthProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Picture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nickname = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Settings_ShowGettingStarted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PricingTierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PricingTierActivatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PricingTierActivatedUntilDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
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
                name: "Invitations",
                columns: table => new
                {
                    YaInvitationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccessType = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Claimed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ClaimedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedMembershipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.YaInvitationID);
                    table.ForeignKey(
                        name: "FK_Invitations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    MembershipID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessType = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => x.MembershipID);
                    table.ForeignKey(
                        name: "FK_Memberships_Tenants_TenantID",
                        column: x => x.TenantID,
                        principalTable: "Tenants",
                        principalColumn: "TenantID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PricingTiers",
                columns: new[] { "PricingTierID", "CreatedBy", "Description", "HasTrial", "LastModifiedBy", "MaxUsers", "MaxVkPeriodicParsingTasks", "Title", "TrialPeriod" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), null, "Бесплатно для всех.", false, null, 1, 1, "Бесплатный", null },
                    { new Guid("00000000-0000-0000-0000-000000000013"), null, "За денежки", true, null, 1, 1, "Платный", 12960000000000L }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AuthProvider", "CreatedDateTime", "Email", "ExternalId", "IsDeleted", "LastModifiedDateTime", "Name", "Nickname", "Picture", "Settings_ShowGettingStarted" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000012"), "auth0", new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(3300), "admin@email.com", "lahblah", false, new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(3306), "Серый кардинал", null, null, true },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "auth0", new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(4535), "user@email.com", "userLahblah", false, new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(4541), "Мышиный король", null, null, true }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "CreatedBy", "IsReadOnly", "LastModifiedBy", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, false, null, "Системный", new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(725), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000001"), 1, 0 });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "CreatedBy", "IsReadOnly", "LastModifiedBy", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), null, false, null, "Уважаемый", new DateTime(2021, 8, 18, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(2300), new DateTime(2021, 9, 17, 5, 33, 47, 468, DateTimeKind.Utc).AddTicks(2307), new Guid("00000000-0000-0000-0000-000000000013"), 1, 1 });

            migrationBuilder.InsertData(
                table: "Memberships",
                columns: new[] { "MembershipID", "AccessType", "CreatedBy", "IsDeleted", "LastModifiedBy", "TenantID", "UserID" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000015"), 8, null, false, null, new Guid("00000000-0000-0000-0000-000000000002"), new Guid("00000000-0000-0000-0000-000000000014") });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_TenantId",
                table: "Invitations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TenantID",
                table: "Memberships",
                column: "TenantID");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserID",
                table: "Memberships",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PricingTierId",
                table: "Tenants",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AuthProvider_ExternalId",
                table: "Users",
                columns: new[] { "AuthProvider", "ExternalId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientInfos");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PricingTiers");
        }
    }
}
