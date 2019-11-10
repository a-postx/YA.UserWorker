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
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.ApiRequest", b =>
                {
                    b.Property<Guid>("ApiRequestID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ApiRequestDateTime");

                    b.Property<string>("Method");

                    b.Property<string>("ResponseBody");

                    b.Property<int?>("ResponseStatusCode");

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("ApiRequestID");

                    b.ToTable("ApiRequests");
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.PricingTier", b =>
                {
                    b.Property<Guid>("PricingTierID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Description")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<string>("Features");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("ResourceLevels");

                    b.Property<string>("Title")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("PricingTierID");

                    b.ToTable("PricingTiers");

                    b.HasData(
                        new
                        {
                            PricingTierID = new Guid("00000000-0000-0000-0000-000000000003"),
                            CreatedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Бесплатно для всех.",
                            LastModifiedDateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Title = "Бесплатный"
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.Tenant", b =>
                {
                    b.Property<Guid>("TenantID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsReadOnly");

                    b.Property<bool>("IsTrial");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<Guid?>("PricingTierID");

                    b.Property<string>("TenantName")
                        .HasMaxLength(128)
                        .IsUnicode(true);

                    b.Property<int>("TenantType");

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("TenantID");

                    b.HasIndex("PricingTierID");

                    b.ToTable("Tenants");

                    b.HasData(
                        new
                        {
                            TenantID = new Guid("00000000-0000-0000-0000-000000000001"),
                            IsActive = true,
                            IsReadOnly = false,
                            IsTrial = false,
                            PricingTierID = new Guid("00000000-0000-0000-0000-000000000003"),
                            TenantName = "Прохожий",
                            TenantType = 1
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.User", b =>
                {
                    b.Property<Guid>("UserID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("Email")
                        .HasMaxLength(128);

                    b.Property<string>("FirstName")
                        .HasMaxLength(255)
                        .IsUnicode(true);

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsPendingActivation");

                    b.Property<DateTime?>("LastLoginDate");

                    b.Property<DateTime>("LastModifiedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<string>("LastName")
                        .HasMaxLength(255)
                        .IsUnicode(true);

                    b.Property<string>("Password")
                        .HasMaxLength(50);

                    b.Property<Guid>("TenantID");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(64);

                    b.Property<byte[]>("tstamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();

                    b.HasKey("UserID");

                    b.HasIndex("TenantID");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserID = new Guid("00000000-0000-0000-0000-000000000011"),
                            CreatedDateTime = new DateTime(2019, 11, 7, 12, 51, 32, 437, DateTimeKind.Utc).AddTicks(5815),
                            Email = "admin@email.com",
                            FirstName = "My",
                            IsActive = true,
                            IsDeleted = false,
                            IsPendingActivation = false,
                            LastModifiedDateTime = new DateTime(2019, 11, 7, 12, 51, 32, 437, DateTimeKind.Utc).AddTicks(5816),
                            LastName = "Admin",
                            Password = "123",
                            TenantID = new Guid("00000000-0000-0000-0000-000000000001"),
                            Username = "admin@ya.ru"
                        });
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.Tenant", b =>
                {
                    b.HasOne("YA.TenantWorker.Core.Entities.PricingTier", "PricingTier")
                        .WithMany("Tenants")
                        .HasForeignKey("PricingTierID")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("YA.TenantWorker.Core.Entities.User", b =>
                {
                    b.HasOne("YA.TenantWorker.Core.Entities.Tenant", "Tenant")
                        .WithMany("Users")
                        .HasForeignKey("TenantID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
