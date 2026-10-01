using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities.MasterDb;

namespace CRM.infrastructure.Data.Context
{
    public class MasterErpDbContext : IdentityDbContext<ApplicationUser>
    {
        public MasterErpDbContext(DbContextOptions<MasterErpDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyDatabase> CompanyDatabases { get; set; }
        public DbSet<Role> AppRoles { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<UserBranch> UserBranches { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<SubscriptionFeature> SubscriptionFeatures { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditions { get; set; }
        public DbSet<UserTermsAcceptance> UserTermsAcceptances { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Company Configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.CompanyId);
                entity.Property(x => x.CompanyCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.LegalName).HasMaxLength(200);
                entity.Property(x => x.TaxId).HasMaxLength(50);
                entity.Property(x => x.RegistrationNumber).HasMaxLength(50);
                entity.Property(x => x.Website).HasMaxLength(200);
                entity.Property(x => x.Industry).HasMaxLength(100);
                entity.HasIndex(x => x.CompanyCode).IsUnique();
            });

            // CompanyDatabase Configuration
            builder.Entity<CompanyDatabase>(entity =>
            {
                entity.HasKey(x => x.CompanyDatabaseId);
                entity.Property(x => x.ServerName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.DatabaseName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.UserName).HasMaxLength(100);
                entity.Property(x => x.PasswordHash).HasMaxLength(500);

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.CompanyDatabases)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT
            });

            // Device Configuration
            builder.Entity<Device>(entity =>
            {
                entity.HasKey(x => x.DeviceId);
                entity.Property(x => x.DeviceCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.DeviceName).HasMaxLength(200).IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Devices)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.CompanyId, x.DeviceCode }).IsUnique();
            });
            // Legacy User entity removed (using ApplicationUser instead)
            // Role Configuration
            builder.Entity<Role>(entity =>
            {
                entity.HasKey(x => x.RoleId);
                entity.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
                entity.Property(x => x.RoleDescription).HasMaxLength(200);
                entity.HasIndex(x => x.RoleName).IsUnique();
            });

            // Branch Configuration
            builder.Entity<Branch>(entity =>
            {
                entity.HasKey(x => x.BranchId);
                entity.Property(x => x.BranchCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.BranchName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Street).HasMaxLength(200).IsRequired();
                entity.Property(x => x.City).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Province).HasMaxLength(50).IsRequired();
                entity.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Country).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Latitude).HasColumnType("decimal(18,6)");
                entity.Property(x => x.Longitude).HasColumnType("decimal(18,6)");
                entity.HasIndex(x => x.BranchCode).IsUnique();

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Branches)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT
            });

            // UserBranch Configuration - FIXED
            builder.Entity<UserBranch>(entity =>
            {
                entity.HasKey(x => x.UserBranchId);

                entity.HasOne<ApplicationUser>(x => x.User)
                    .WithMany(u => u.UserBranches)
                    .HasForeignKey(x => x.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);  // ← RESTRICT

                entity.HasOne<Branch>(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT
            });

            // SubscriptionPlan Configuration
            builder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(x => x.PlanId);
                entity.Property(x => x.PlanName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.PlanCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.PricePerMonth).HasColumnType("decimal(18,2)");
                entity.Property(x => x.PricePerYear).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => x.PlanName).IsUnique();
                entity.HasIndex(x => x.PlanCode).IsUnique();
            });

            // TenantSubscription Configuration
            builder.Entity<TenantSubscription>(entity =>
            {
                entity.HasKey(x => x.TenantSubscriptionId);
                entity.Property(x => x.PaymentStatus).HasMaxLength(20).IsRequired();
                entity.Property(x => x.AmountPaid).HasColumnType("decimal(18,2)");

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.TenantSubscriptions)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT

                entity.HasOne(x => x.Plan)
                    .WithMany(x => x.TenantSubscriptions)
                    .HasForeignKey(x => x.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SubscriptionFeature Configuration
            builder.Entity<SubscriptionFeature>(entity =>
            {
                entity.HasKey(x => x.SubscriptionFeatureId);
                entity.Property(x => x.FeatureKey).HasMaxLength(100).IsRequired();

                entity.HasOne(x => x.Plan)
                    .WithMany(x => x.SubscriptionFeatures)
                    .HasForeignKey(x => x.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TermsAndConditions Configuration
            builder.Entity<TermsAndConditions>(entity =>
            {
                entity.HasKey(x => x.TermsId);
                entity.Property(x => x.Version).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.TermsAndConditions)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT
            });

            // UserTermsAcceptance Configuration
            builder.Entity<UserTermsAcceptance>(entity =>
            {
                entity.HasKey(x => x.UserTermsAcceptanceId);
                entity.Property(x => x.IpAddress).HasMaxLength(45);
                entity.Property(x => x.UserAgent).HasMaxLength(500);

                entity.HasOne<ApplicationUser>(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Terms)
                    .WithMany(x => x.UserTermsAcceptances)
                    .HasForeignKey(x => x.TermsId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT
            });

            // AuditLog Configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.AuditId);
                entity.Property(x => x.Action).HasMaxLength(100).IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany(x => x.AuditLogs)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);  // ← RESTRICT

                entity.HasOne<ApplicationUser>(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}