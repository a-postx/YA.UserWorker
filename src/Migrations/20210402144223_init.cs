﻿using System;
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
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
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
                    tstamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantID);
                    table.ForeignKey(
                        name: "FK_Tenants_PricingTiers_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierID");
                    table.ForeignKey(
                        name: "FK_Tenants_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
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
                columns: new[] { "PricingTierID", "Description", "HasTrial", "MaxUsers", "Title", "TrialPeriod" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Бесплатно для всех.", false, 1, "Бесплатный", null },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "За денежки", true, 1, "Платный", 12960000000000L }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AuthProvider", "CreatedDateTime", "Email", "ExternalId", "IsDeleted", "LastModifiedDateTime", "Name", "Nickname", "Picture", "Settings_ShowGettingStarted" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000012"), "auth0", new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(8036), "admin@email.com", "lahblah", false, new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(8044), "Серый кардинал", null, null, true },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "auth0", new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(9586), "user@email.com", "userLahblah", false, new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(9594), "Мышиный король", null, null, true }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "IsReadOnly", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type", "UserID" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), false, "Системный", new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(4347), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("00000000-0000-0000-0000-000000000001"), 1, 0, null });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "IsReadOnly", "Name", "PricingTierActivatedDateTime", "PricingTierActivatedUntilDateTime", "PricingTierId", "Status", "Type", "UserID" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), false, "Уважаемый", new DateTime(2021, 4, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(6683), new DateTime(2021, 5, 2, 14, 42, 22, 631, DateTimeKind.Utc).AddTicks(6698), new Guid("00000000-0000-0000-0000-000000000013"), 1, 1, null });

            migrationBuilder.InsertData(
                table: "Memberships",
                columns: new[] { "MembershipID", "AccessType", "IsDeleted", "TenantID", "UserID" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000015"), 4, false, new Guid("00000000-0000-0000-0000-000000000002"), new Guid("00000000-0000-0000-0000-000000000014") });

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
                name: "IX_Tenants_UserID",
                table: "Tenants",
                column: "UserID");

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
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "PricingTiers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}