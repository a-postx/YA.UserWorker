﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YA.TenantWorker.Infrastructure.Data;

namespace YA.TenantWorker.Migrations
{
    [DbContext(typeof(TenantWorkerDbContext))]
    partial class TenantWorkerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.ApiRequest", b =>
                {
                    b.Property<Guid>("ApiRequestID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ApiRequestDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Method")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ResponseBody")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ResponseStatusCode")
                        .HasColumnType("int");

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ApiRequestID");

                    b.ToTable("ApiRequests");
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.PricingTier", b =>
                {
                    b.Property<Guid>("PricingTierID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<bool>("HasTrial")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<int>("MaxScheduledTasks")
                        .HasColumnType("int");

                    b.Property<int>("MaxUsers")
                        .HasColumnType("int");

                    b.Property<int>("MaxVkCommunities")
                        .HasColumnType("int");

                    b.Property<int>("MaxVkCommunitySize")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<long?>("TrialPeriod")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("PricingTierID");

                    b.ToTable("PricingTiers");

                    b.HasData(
                        new
                        {
                            PricingTierID = new Guid("00000000-0000-0000-0000-000000000001"),
                            CreatedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Бесплатно для всех.",
                            HasTrial = false,
                            LastModifiedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            MaxScheduledTasks = 0,
                            MaxUsers = 1,
                            MaxVkCommunities = 1,
                            MaxVkCommunitySize = 1000,
                            Title = "Бесплатный"
                        },
                        new
                        {
                            PricingTierID = new Guid("00000000-0000-0000-0000-000000000013"),
                            CreatedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "За денежки",
                            HasTrial = true,
                            LastModifiedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            MaxScheduledTasks = 1,
                            MaxUsers = 1,
                            MaxVkCommunities = 1,
                            MaxVkCommunitySize = 10000,
                            Title = "Платный",
                            TrialPeriod = 12960000000000L
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.Tenant", b =>
                {
                    b.Property<Guid>("TenantID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AuthProvider")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<string>("ExternalId")
                        .HasColumnType("nvarchar(256)")
                        .HasMaxLength(256)
                        .IsUnicode(true);

                    b.Property<bool>("IsReadOnly")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(512)")
                        .HasMaxLength(512)
                        .IsUnicode(true);

                    b.Property<DateTime>("PricingTierActivatedDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("PricingTierActivatedUntilDateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PricingTierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TenantID");

                    b.HasIndex("PricingTierId");

                    b.ToTable("Tenants");

                    b.HasData(
                        new
                        {
                            TenantID = new Guid("00000000-0000-0000-0000-000000000001"),
                            IsReadOnly = false,
                            Name = "Системный",
                            PricingTierActivatedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(3717),
                            PricingTierActivatedUntilDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            PricingTierId = new Guid("00000000-0000-0000-0000-000000000001"),
                            Status = 1,
                            Type = 0
                        },
                        new
                        {
                            TenantID = new Guid("00000000-0000-0000-0000-000000000002"),
                            IsReadOnly = false,
                            Name = "Уважаемый",
                            PricingTierActivatedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(5427),
                            PricingTierActivatedUntilDateTime = new DateTime(2020, 12, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(5436),
                            PricingTierId = new Guid("00000000-0000-0000-0000-000000000013"),
                            Status = 1,
                            Type = 1
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.User", b =>
                {
                    b.Property<Guid>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(128)")
                        .HasMaxLength(128);

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255)
                        .IsUnicode(true);

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPendingActivation")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255)
                        .IsUnicode(true);

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Role")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)")
                        .HasMaxLength(64);

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("UserID");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserID = new Guid("00000000-0000-0000-0000-000000000011"),
                            CreatedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(6671),
                            Email = "admin@email.com",
                            FirstName = "My",
                            IsActive = true,
                            IsDeleted = false,
                            IsPendingActivation = false,
                            LastModifiedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(6678),
                            LastName = "Admin",
                            Password = "123",
                            Role = "Administrator",
                            TenantId = new Guid("00000000-0000-0000-0000-000000000001"),
                            Username = "admin@ya.ru"
                        },
                        new
                        {
                            UserID = new Guid("00000000-0000-0000-0000-000000000012"),
                            CreatedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(8626),
                            Email = "user@email.com",
                            FirstName = "My",
                            IsActive = true,
                            IsDeleted = false,
                            IsPendingActivation = false,
                            LastModifiedDateTime = new DateTime(2020, 11, 11, 13, 34, 48, 440, DateTimeKind.Utc).AddTicks(8635),
                            LastName = "User",
                            Password = "123",
                            Role = "User",
                            TenantId = new Guid("00000000-0000-0000-0000-000000000001"),
                            Username = "user@ya.ru"
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.YaClientInfo", b =>
                {
                    b.Property<Guid>("YaClientInfoID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Browser")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(true);

                    b.Property<string>("BrowserVersion")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("CountryName")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(true);

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("DeviceModel")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(true);

                    b.Property<string>("IpAddress")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Os")
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200)
                        .IsUnicode(true);

                    b.Property<string>("OsVersion")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("RegionName")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500)
                        .IsUnicode(true);

                    b.Property<string>("ScreenResolution")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(320)")
                        .HasMaxLength(320)
                        .IsUnicode(true);

                    b.Property<string>("ViewportSize")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("YaClientInfoID");

                    b.HasIndex("TenantId");

                    b.ToTable("ClientInfos");
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.Tenant", b =>
                {
                    b.HasOne("YA.TenantWorker.Core.Entities.PricingTier", "PricingTier")
                        .WithMany("Tenants")
                        .HasForeignKey("PricingTierId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.User", b =>
                {
                    b.HasOne("YA.TenantWorker.Core.Entities.Tenant", "Tenant")
                        .WithMany("Users")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.YaClientInfo", b =>
                {
                    b.HasOne("YA.TenantWorker.Core.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
