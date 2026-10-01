SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Products] (
    [ProductId] int NOT NULL IDENTITY,
    [ProductCode] nvarchar(50) NOT NULL,
    [ProductName] nvarchar(200) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId])
);

CREATE UNIQUE INDEX [IX_Products_ProductCode] ON [Products] ([ProductCode]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260911132626_InitialTenantErp', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Customers] (
    [CustomerId] int NOT NULL IDENTITY,
    [CustomerCode] nvarchar(50) NOT NULL,
    [CustomerType] nvarchar(20) NOT NULL DEFAULT N'Individual',
    [CompanyName] nvarchar(max) NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NULL,
    [PhonePrimary] nvarchar(20) NOT NULL,
    [PhoneSecondary] nvarchar(max) NULL,
    [BirthDate] datetime2 NULL,
    [Gender] nvarchar(max) NULL,
    [PreferredLanguage] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
);

CREATE TABLE [GarmentTypes] (
    [GarmentTypeId] int NOT NULL IDENTITY,
    [GarmentCode] nvarchar(20) NOT NULL,
    [GarmentName] nvarchar(100) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DefaultPrice] decimal(18,2) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_GarmentTypes] PRIMARY KEY ([GarmentTypeId])
);

CREATE TABLE [LoyaltySettings] (
    [LoyaltySettingId] int NOT NULL IDENTITY,
    [PointsPerOrder] decimal(5,2) NOT NULL,
    [PointsPerDollar] decimal(5,2) NOT NULL,
    [RedeemPointsRequired] decimal(5,2) NOT NULL,
    [RedeemDiscountAmount] decimal(18,2) NOT NULL,
    [PointsExpiryDays] int NULL,
    [IsActive] bit NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_LoyaltySettings] PRIMARY KEY ([LoyaltySettingId])
);

CREATE TABLE [LoyaltyTiers] (
    [LoyaltyTierId] int NOT NULL IDENTITY,
    [TierName] nvarchar(50) NOT NULL,
    [MinPoints] int NOT NULL,
    [MaxPoints] int NULL,
    [DiscountPercentage] decimal(18,2) NOT NULL,
    [PointsMultiplier] decimal(18,2) NOT NULL,
    [BenefitsJSON] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LoyaltyTiers] PRIMARY KEY ([LoyaltyTierId])
);

CREATE TABLE [OrderStatuses] (
    [StatusId] int NOT NULL IDENTITY,
    [StatusCode] nvarchar(10) NOT NULL,
    [StatusName] nvarchar(50) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsFinal] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [ColorCode] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_OrderStatuses] PRIMARY KEY ([StatusId])
);

CREATE TABLE [PaymentMethods] (
    [PaymentMethodId] int NOT NULL IDENTITY,
    [MethodCode] nvarchar(20) NOT NULL,
    [MethodName] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_PaymentMethods] PRIMARY KEY ([PaymentMethodId])
);

CREATE TABLE [ServiceCategories] (
    [ServiceCategoryId] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(100) NOT NULL,
    [CategoryCode] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [IconPath] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ServiceCategories] PRIMARY KEY ([ServiceCategoryId])
);

CREATE TABLE [CustomerAddresses] (
    [CustomerAddressId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [AddressType] nvarchar(max) NOT NULL,
    [AddressLine1] nvarchar(max) NOT NULL,
    [AddressLine2] nvarchar(max) NULL,
    [City] nvarchar(max) NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [PostalCode] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    [Latitude] decimal(18,2) NULL,
    [Longitude] decimal(18,2) NULL,
    [IsDefault] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([CustomerAddressId]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
);

CREATE TABLE [CustomerLoyalties] (
    [CustomerLoyaltyId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [LoyaltyTierId] int NULL,
    [CurrentPoints] int NOT NULL,
    [TotalPointsEarned] int NOT NULL,
    [TotalPointsRedeemed] int NOT NULL,
    [LifetimeSpend] decimal(18,2) NOT NULL,
    [LastActivityDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CustomerLoyalties] PRIMARY KEY ([CustomerLoyaltyId]),
    CONSTRAINT [FK_CustomerLoyalties_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CustomerLoyalties_LoyaltyTiers_LoyaltyTierId] FOREIGN KEY ([LoyaltyTierId]) REFERENCES [LoyaltyTiers] ([LoyaltyTierId])
);

CREATE TABLE [Orders] (
    [OrderId] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(50) NOT NULL,
    [CustomerId] int NOT NULL,
    [BranchId] int NOT NULL,
    [OrderType] nvarchar(max) NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [StatusId] int NOT NULL,
    [Priority] nvarchar(max) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [TotalWeight] decimal(10,2) NULL,
    [TotalItems] int NOT NULL,
    [SpecialInstructions] nvarchar(max) NULL,
    [PickupDate] datetime2 NULL,
    [DeliveryDate] datetime2 NULL,
    [ActualPickupDate] datetime2 NULL,
    [ActualDeliveryDate] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedByUserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([OrderId]),
    CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_OrderStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [OrderStatuses] ([StatusId]) ON DELETE NO ACTION
);

CREATE TABLE [Services] (
    [ServiceId] int NOT NULL IDENTITY,
    [ServiceCategoryId] int NOT NULL,
    [ServiceCode] nvarchar(20) NOT NULL,
    [ServiceName] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [PricePerUnit] decimal(18,2) NULL,
    [UnitOfMeasure] nvarchar(max) NOT NULL,
    [ProcessingTimeHours] int NOT NULL,
    [IsExpressService] bit NOT NULL,
    [ExpressMultiplier] decimal(18,2) NULL,
    [RequiresSpecialHandling] bit NOT NULL,
    [HandlingInstructions] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId]),
    CONSTRAINT [FK_Services_ServiceCategories_ServiceCategoryId] FOREIGN KEY ([ServiceCategoryId]) REFERENCES [ServiceCategories] ([ServiceCategoryId]) ON DELETE CASCADE
);

CREATE TABLE [LoyaltyTransactions] (
    [LoyaltyTransactionId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [OrderId] int NULL,
    [TransactionType] nvarchar(max) NOT NULL,
    [PointsChange] int NOT NULL,
    [PointsBalance] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [Reference] nvarchar(max) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_LoyaltyTransactions] PRIMARY KEY ([LoyaltyTransactionId]),
    CONSTRAINT [FK_LoyaltyTransactions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LoyaltyTransactions_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE NO ACTION
);

CREATE TABLE [OrderPayments] (
    [OrderPaymentId] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [PaymentMethodId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ReferenceNumber] nvarchar(max) NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_OrderPayments] PRIMARY KEY ([OrderPaymentId]),
    CONSTRAINT [FK_OrderPayments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderPayments_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([PaymentMethodId]) ON DELETE NO ACTION
);

CREATE TABLE [OrderStatusHistories] (
    [OrderStatusHistoryId] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [StatusId] int NOT NULL,
    [ChangedByUserId] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [ChangedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY ([OrderStatusHistoryId]),
    CONSTRAINT [FK_OrderStatusHistories_OrderStatuses_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [OrderStatuses] ([StatusId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_OrderStatusHistories_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE
);

CREATE TABLE [OrderItems] (
    [OrderItemId] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ServiceId] int NOT NULL,
    [GarmentTypeId] int NULL,
    [Quantity] decimal(10,2) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [SpecialInstructions] nvarchar(max) NULL,
    [IsCompleted] bit NOT NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([OrderItemId]),
    CONSTRAINT [FK_OrderItems_GarmentTypes_GarmentTypeId] FOREIGN KEY ([GarmentTypeId]) REFERENCES [GarmentTypes] ([GarmentTypeId]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE NO ACTION
);

CREATE TABLE [PriceMatrices] (
    [PriceMatrixId] int NOT NULL IDENTITY,
    [ServiceId] int NOT NULL,
    [GarmentTypeId] int NULL,
    [ServiceType] nvarchar(max) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [MinQuantity] decimal(18,2) NULL,
    [MaxQuantity] decimal(18,2) NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_PriceMatrices] PRIMARY KEY ([PriceMatrixId]),
    CONSTRAINT [FK_PriceMatrices_GarmentTypes_GarmentTypeId] FOREIGN KEY ([GarmentTypeId]) REFERENCES [GarmentTypes] ([GarmentTypeId]),
    CONSTRAINT [FK_PriceMatrices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
);

CREATE INDEX [IX_CustomerAddresses_CustomerId] ON [CustomerAddresses] ([CustomerId]);

CREATE UNIQUE INDEX [IX_CustomerLoyalties_CustomerId] ON [CustomerLoyalties] ([CustomerId]);

CREATE INDEX [IX_CustomerLoyalties_LoyaltyTierId] ON [CustomerLoyalties] ([LoyaltyTierId]);

CREATE UNIQUE INDEX [IX_Customers_CustomerCode] ON [Customers] ([CustomerCode]);

CREATE UNIQUE INDEX [IX_GarmentTypes_GarmentCode] ON [GarmentTypes] ([GarmentCode]);

CREATE UNIQUE INDEX [IX_LoyaltyTiers_TierName] ON [LoyaltyTiers] ([TierName]);

CREATE INDEX [IX_LoyaltyTransactions_CustomerId] ON [LoyaltyTransactions] ([CustomerId]);

CREATE INDEX [IX_LoyaltyTransactions_OrderId] ON [LoyaltyTransactions] ([OrderId]);

CREATE INDEX [IX_OrderItems_GarmentTypeId] ON [OrderItems] ([GarmentTypeId]);

CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);

CREATE INDEX [IX_OrderItems_ServiceId] ON [OrderItems] ([ServiceId]);

CREATE INDEX [IX_OrderPayments_OrderId] ON [OrderPayments] ([OrderId]);

CREATE INDEX [IX_OrderPayments_PaymentMethodId] ON [OrderPayments] ([PaymentMethodId]);

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);

CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);

CREATE INDEX [IX_Orders_StatusId] ON [Orders] ([StatusId]);

CREATE UNIQUE INDEX [IX_OrderStatuses_StatusCode] ON [OrderStatuses] ([StatusCode]);

CREATE INDEX [IX_OrderStatusHistories_OrderId] ON [OrderStatusHistories] ([OrderId]);

CREATE INDEX [IX_OrderStatusHistories_StatusId] ON [OrderStatusHistories] ([StatusId]);

CREATE UNIQUE INDEX [IX_PaymentMethods_MethodCode] ON [PaymentMethods] ([MethodCode]);

CREATE INDEX [IX_PriceMatrices_GarmentTypeId] ON [PriceMatrices] ([GarmentTypeId]);

CREATE INDEX [IX_PriceMatrices_ServiceId] ON [PriceMatrices] ([ServiceId]);

CREATE UNIQUE INDEX [IX_ServiceCategories_CategoryName] ON [ServiceCategories] ([CategoryName]);

CREATE INDEX [IX_Services_ServiceCategoryId] ON [Services] ([ServiceCategoryId]);

CREATE UNIQUE INDEX [IX_Services_ServiceCode] ON [Services] ([ServiceCode]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260911141052_AddOrderManagementToTenantErp', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderStatusHistories]') AND [c].[name] = N'ChangedByUserId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [OrderStatusHistories] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [OrderStatusHistories] ALTER COLUMN [ChangedByUserId] nvarchar(max) NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'CreatedByUserId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Orders] ALTER COLUMN [CreatedByUserId] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [AddressLine1] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [AddressLine2] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [City] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [Country] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [LastOrderDate] datetime2 NULL;

ALTER TABLE [Customers] ADD [LifetimeSpend] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Customers] ADD [LoyaltyPoints] int NOT NULL DEFAULT 0;

ALTER TABLE [Customers] ADD [PostalCode] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [State] nvarchar(max) NULL;

ALTER TABLE [Customers] ADD [TotalOrders] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260913075941_ChangeUserIdToGuidString', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [CustomerInteractions] (
    [InteractionId] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [InteractionType] int NOT NULL,
    [Subject] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Status] int NOT NULL,
    [Priority] int NOT NULL,
    [Rating] int NULL,
    [AssignedToUserId] nvarchar(max) NULL,
    [CreatedByUserId] nvarchar(max) NOT NULL,
    [UpdatedByUserId] nvarchar(max) NULL,
    [ResolutionNotes] nvarchar(max) NULL,
    [CustomerResponse] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [ResolvedAt] datetime2 NULL,
    [ClosedAt] datetime2 NULL,
    CONSTRAINT [PK_CustomerInteractions] PRIMARY KEY ([InteractionId]),
    CONSTRAINT [FK_CustomerInteractions_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
);

CREATE TABLE [CustomerInteractionStatusHistories] (
    [StatusHistoryId] int NOT NULL IDENTITY,
    [InteractionId] int NOT NULL,
    [FromStatus] int NULL,
    [ToStatus] int NOT NULL,
    [ChangedByUserId] nvarchar(max) NOT NULL,
    [ChangedAt] datetime2 NOT NULL,
    [Notes] nvarchar(max) NULL,
    CONSTRAINT [PK_CustomerInteractionStatusHistories] PRIMARY KEY ([StatusHistoryId]),
    CONSTRAINT [FK_CustomerInteractionStatusHistories_CustomerInteractions_InteractionId] FOREIGN KEY ([InteractionId]) REFERENCES [CustomerInteractions] ([InteractionId]) ON DELETE CASCADE
);

CREATE INDEX [IX_CustomerInteractions_CustomerId] ON [CustomerInteractions] ([CustomerId]);

CREATE INDEX [IX_CustomerInteractionStatusHistories_InteractionId] ON [CustomerInteractionStatusHistories] ([InteractionId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915032521_AddCustomerInteractions', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'AddressLine1');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Customers] DROP COLUMN [AddressLine1];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'AddressLine2');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Customers] DROP COLUMN [AddressLine2];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'BirthDate');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Customers] DROP COLUMN [BirthDate];

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'Gender');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Customers] DROP COLUMN [Gender];

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customers]') AND [c].[name] = N'PhoneSecondary');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Customers] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [Customers] DROP COLUMN [PhoneSecondary];

ALTER TABLE [Customers] ADD [Street] nvarchar(200) NULL;

ALTER TABLE [Customers] ADD [Village] nvarchar(200) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915120000_StreamlineCustomerFields', N'10.0.0');

COMMIT;
GO

