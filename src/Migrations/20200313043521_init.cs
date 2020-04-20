﻿using System;
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
                    ApiRequestID = table.Column<Guid>(nullable: false),
                    ApiRequestDateTime = table.Column<DateTime>(nullable: false),
                    Method = table.Column<string>(nullable: true),
                    ResponseStatusCode = table.Column<int>(nullable: true),
                    ResponseBody = table.Column<string>(nullable: true),
                    tstamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequests", x => x.ApiRequestID);
                });

            migrationBuilder.CreateTable(
                name: "PricingTiers",
                columns: table => new
                {
                    PricingTierID = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 128, nullable: true),
                    Description = table.Column<string>(maxLength: 128, nullable: true),
                    HasTrial = table.Column<bool>(nullable: false),
                    TrialPeriod = table.Column<long>(nullable: true),
                    MaxUsers = table.Column<int>(nullable: false),
                    MaxVkCommunities = table.Column<int>(nullable: false),
                    MaxVkCommunitySize = table.Column<int>(nullable: false),
                    MaxScheduledTasks = table.Column<int>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.PricingTierID);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantID = table.Column<Guid>(nullable: false),
                    TenantName = table.Column<string>(maxLength: 128, nullable: true),
                    TenantType = table.Column<int>(nullable: false),
                    PricingTierID = table.Column<Guid>(nullable: true),
                    PricingTierActivatedDateTime = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsReadOnly = table.Column<bool>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantID);
                    table.ForeignKey(
                        name: "FK_Tenants_PricingTiers_PricingTierID",
                        column: x => x.PricingTierID,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    TenantID = table.Column<Guid>(nullable: false),
                    Username = table.Column<string>(maxLength: 64, nullable: false),
                    Password = table.Column<string>(maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    Email = table.Column<string>(maxLength: 128, nullable: true),
                    Role = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    IsPendingActivation = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginDate = table.Column<DateTime>(nullable: true),
                    LastModifiedDateTime = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    tstamp = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantID",
                        column: x => x.TenantID,
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
                columns: new[] { "TenantID", "IsActive", "IsReadOnly", "PricingTierActivatedDateTime", "PricingTierID", "TenantName", "TenantType" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), true, false, new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(2126), new Guid("00000000-0000-0000-0000-000000000001"), "Системный", 0 });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "IsActive", "IsReadOnly", "PricingTierActivatedDateTime", "PricingTierID", "TenantName", "TenantType" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), true, false, new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(3903), new Guid("00000000-0000-0000-0000-000000000013"), "Уважаемый", 1 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedDateTime", "Email", "FirstName", "IsActive", "IsDeleted", "IsPendingActivation", "LastLoginDate", "LastModifiedDateTime", "LastName", "Password", "Role", "TenantID", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000011"), new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(5273), "admin@email.com", "My", true, false, false, null, new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(5281), "Admin", "123", "Administrator", new Guid("00000000-0000-0000-0000-000000000001"), "admin@ya.ru" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedDateTime", "Email", "FirstName", "IsActive", "IsDeleted", "IsPendingActivation", "LastLoginDate", "LastModifiedDateTime", "LastName", "Password", "Role", "TenantID", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000012"), new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(7301), "user@email.com", "My", true, false, false, null, new DateTime(2020, 3, 13, 4, 35, 20, 859, DateTimeKind.Utc).AddTicks(7309), "User", "123", "User", new Guid("00000000-0000-0000-0000-000000000001"), "user@ya.ru" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PricingTierID",
                table: "Tenants",
                column: "PricingTierID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantID",
                table: "Users",
                column: "TenantID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiRequests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "PricingTiers");
        }
    }
}