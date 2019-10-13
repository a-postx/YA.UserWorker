﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YA.TenantWorker.Migrations
{
    public partial class initial : Migration
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
                    ResourceLevels = table.Column<string>(nullable: true),
                    Features = table.Column<string>(nullable: true),
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
                    TenantType = table.Column<int>(nullable: false),
                    TenantName = table.Column<string>(maxLength: 128, nullable: true),
                    PricingTierID = table.Column<Guid>(nullable: true),
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
                        principalColumn: "PricingTierID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    TenantID = table.Column<Guid>(nullable: false),
                    Username = table.Column<string>(maxLength: 64, nullable: false),
                    FirstName = table.Column<string>(maxLength: 255, nullable: true),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    Email = table.Column<string>(maxLength: 128, nullable: true),
                    Active = table.Column<bool>(nullable: false),
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
                columns: new[] { "PricingTierID", "Description", "Features", "ResourceLevels", "Title" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000003"), "Бесплатно для всех.", null, null, "Бесплатный" });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantID", "IsReadOnly", "PricingTierID", "TenantName", "TenantType" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), false, new Guid("00000000-0000-0000-0000-000000000003"), "Прохожий", 1 });

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
