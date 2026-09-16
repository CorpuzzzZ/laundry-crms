using Microsoft.EntityFrameworkCore;
using CRM.Domain.Entities.TenantDb;

namespace CRM.infrastructure.Data.Context
{
    public class TenantErpDbContext : DbContext
    {
        public TenantErpDbContext(DbContextOptions<TenantErpDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        // ----- ORDER MANAGEMENT -----
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

        // ----- CUSTOMERS (for orders) -----
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerInteraction> CustomerInteractions => Set<CustomerInteraction>();
        public DbSet<CustomerInteractionStatusHistory> CustomerInteractionStatusHistories => Set<CustomerInteractionStatusHistory>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
        public DbSet<CustomerLoyalty> CustomerLoyalties => Set<CustomerLoyalty>();
        public DbSet<LoyaltyTier> LoyaltyTiers => Set<LoyaltyTier>();
        public DbSet<LoyaltySetting> LoyaltySettings => Set<LoyaltySetting>();
        public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();

        // ----- SERVICES (for order items) -----
        public DbSet<Service> Services => Set<Service>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<ServiceAddOn> ServiceAddOns => Set<ServiceAddOn>();
        public DbSet<GarmentType> GarmentTypes => Set<GarmentType>();
        public DbSet<PriceMatrix> PriceMatrices => Set<PriceMatrix>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Products
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.ProductId);
                entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.HasIndex(x => x.ProductCode).IsUnique();
            });

            // Customer
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.CustomerId);
                entity.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CustomerType).HasMaxLength(20).IsRequired().HasDefaultValue("Individual");
                entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(255);
                entity.Property(x => x.PhonePrimary).HasMaxLength(20).IsRequired();
                entity.HasIndex(x => x.CustomerCode).IsUnique();
            });

            // Order
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.OrderId);
                entity.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.Property(x => x.TotalWeight).HasPrecision(10, 2);
                entity.HasIndex(x => x.OrderNumber).IsUnique();

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Status)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderStatus
            builder.Entity<OrderStatus>(entity =>
            {
                entity.HasKey(x => x.StatusId);
                entity.Property(x => x.StatusCode).HasMaxLength(10).IsRequired();
                entity.Property(x => x.StatusName).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.StatusCode).IsUnique();
            });

            // OrderItem
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(x => x.OrderItemId);
                entity.Property(x => x.Quantity).HasPrecision(10, 2);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Service)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderStatusHistory
            builder.Entity<OrderStatusHistory>(entity =>
            {
                entity.HasKey(x => x.OrderStatusHistoryId);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.StatusHistory)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Status)
                    .WithMany(x => x.StatusHistory)
                    .HasForeignKey(x => x.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderPayment
            builder.Entity<OrderPayment>(entity =>
            {
                entity.HasKey(x => x.OrderPaymentId);
                entity.Property(x => x.Amount).HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.Payments)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.PaymentMethod)
                    .WithMany(x => x.OrderPayments)
                    .HasForeignKey(x => x.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentMethod
            builder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(x => x.PaymentMethodId);
                entity.Property(x => x.MethodCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.MethodName).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.MethodCode).IsUnique();
            });

            // Service
            builder.Entity<Service>(entity =>
            {
                entity.HasKey(x => x.ServiceId);
                entity.Property(x => x.ServiceCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.BasePrice).HasPrecision(18, 2);
                entity.HasIndex(x => x.ServiceCode).IsUnique();
            });

            // ServiceAddOn
            builder.Entity<ServiceAddOn>(entity =>
            {
                entity.HasKey(x => x.ServiceAddOnId);
                entity.Property(x => x.AddOnName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.AdditionalPrice).HasPrecision(18, 2);

                entity.HasOne(x => x.Service)
                    .WithMany(s => s.AddOns)
                    .HasForeignKey(x => x.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.ServiceId);
            });

            // ServiceCategory
            builder.Entity<ServiceCategory>(entity =>
            {
                entity.HasKey(x => x.ServiceCategoryId);
                entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.CategoryName).IsUnique();
            });

            // GarmentType
            builder.Entity<GarmentType>(entity =>
            {
                entity.HasKey(x => x.GarmentTypeId);
                entity.Property(x => x.GarmentCode).HasMaxLength(20).IsRequired();
                entity.Property(x => x.GarmentName).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.GarmentCode).IsUnique();
            });

            // PriceMatrix
            builder.Entity<PriceMatrix>(entity =>
            {
                entity.HasKey(x => x.PriceMatrixId);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            });

            // CustomerAddress
            builder.Entity<CustomerAddress>(entity =>
            {
                entity.HasKey(x => x.CustomerAddressId);
                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.CustomerAddresses)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CustomerLoyalty
            builder.Entity<CustomerLoyalty>(entity =>
            {
                entity.HasKey(x => x.CustomerLoyaltyId);
                entity.Property(x => x.LifetimeSpend).HasPrecision(18, 2);
                entity.HasOne(x => x.Customer)
                    .WithOne(x => x.CustomerLoyalty)
                    .HasForeignKey<CustomerLoyalty>(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LoyaltyTier
            builder.Entity<LoyaltyTier>(entity =>
            {
                entity.HasKey(x => x.LoyaltyTierId);
                entity.Property(x => x.TierName).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.TierName).IsUnique();
            });

            // LoyaltySetting
            builder.Entity<LoyaltySetting>(entity =>
            {
                entity.HasKey(x => x.LoyaltySettingId);
                entity.Property(x => x.PointsPerOrder).HasPrecision(5, 2);
                entity.Property(x => x.PointsPerDollar).HasPrecision(5, 2);
                entity.Property(x => x.RedeemPointsRequired).HasPrecision(5, 2);
                entity.Property(x => x.RedeemDiscountAmount).HasPrecision(18, 2);
            });

            // LoyaltyTransaction
            builder.Entity<LoyaltyTransaction>(entity =>
            {
                entity.HasKey(x => x.LoyaltyTransactionId);
                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.LoyaltyTransactions)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Order)
                    .WithMany(x => x.LoyaltyTransactions)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
