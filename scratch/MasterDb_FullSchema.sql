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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [Companies] (
    [CompanyId] int NOT NULL IDENTITY,
    [CompanyCode] nvarchar(50) NOT NULL,
    [CompanyName] nvarchar(200) NOT NULL,
    [Address] nvarchar(500) NULL,
    [Phone] nvarchar(20) NULL,
    [Email] nvarchar(200) NULL,
    [TaxId] nvarchar(50) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([CompanyId])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [CompanyId] int NOT NULL,
    [RefreshToken] nvarchar(500) NULL,
    [RefreshTokenExpiryTime] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [CompanyDatabases] (
    [CompanyDatabaseId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [ServerName] nvarchar(200) NOT NULL,
    [DatabaseName] nvarchar(200) NOT NULL,
    [UserId] nvarchar(100) NULL,
    [PasswordHash] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CompanyDatabases] PRIMARY KEY ([CompanyDatabaseId]),
    CONSTRAINT [FK_CompanyDatabases_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE INDEX [IX_AspNetUsers_CompanyId] ON [AspNetUsers] ([CompanyId]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_Companies_CompanyCode] ON [Companies] ([CompanyCode]);

CREATE INDEX [IX_CompanyDatabases_CompanyId] ON [CompanyDatabases] ([CompanyId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260907035908_InitialCreate', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260907051441_InitialMasterDb', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Companies_CompanyId];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Companies]') AND [c].[name] = N'Address');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Companies] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Companies] DROP COLUMN [Address];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Companies]') AND [c].[name] = N'Phone');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Companies] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Companies] DROP COLUMN [Phone];

EXEC sp_rename N'[CompanyDatabases].[UserId]', N'UserName', 'COLUMN';

EXEC sp_rename N'[Companies].[Email]', N'Website', 'COLUMN';

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CompanyDatabases]') AND [c].[name] = N'PasswordHash');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [CompanyDatabases] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [CompanyDatabases] ALTER COLUMN [PasswordHash] nvarchar(500) NULL;

ALTER TABLE [Companies] ADD [Industry] nvarchar(100) NULL;

ALTER TABLE [Companies] ADD [LegalName] nvarchar(200) NULL;

ALTER TABLE [Companies] ADD [RegistrationNumber] nvarchar(50) NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'RefreshToken');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [AspNetUsers] ALTER COLUMN [RefreshToken] nvarchar(max) NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LastName');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [AspNetUsers] ALTER COLUMN [LastName] nvarchar(max) NOT NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'FirstName');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [AspNetUsers] ALTER COLUMN [FirstName] nvarchar(max) NOT NULL;

CREATE TABLE [AppRoles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    [RoleDescription] nvarchar(200) NULL,
    [PermissionJSON] nvarchar(max) NOT NULL,
    [IsSystemRole] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AppRoles] PRIMARY KEY ([RoleId])
);

CREATE TABLE [Devices] (
    [DeviceId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [DeviceCode] nvarchar(50) NOT NULL,
    [DeviceName] nvarchar(200) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY ([DeviceId]),
    CONSTRAINT [FK_Devices_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [SubscriptionPlans] (
    [PlanId] int NOT NULL IDENTITY,
    [PlanName] nvarchar(100) NOT NULL,
    [PlanCode] nvarchar(20) NOT NULL,
    [Description] nvarchar(max) NULL,
    [PricePerMonth] decimal(18,2) NOT NULL,
    [PricePerYear] decimal(18,2) NULL,
    [MaxUsers] int NOT NULL,
    [MaxBranches] int NOT NULL,
    [MaxOrdersPerMonth] int NULL,
    [MaxStorageGB] int NULL,
    [IsActive] bit NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([PlanId])
);

CREATE TABLE [TermsAndConditions] (
    [TermsId] int NOT NULL IDENTITY,
    [CompanyId] int NULL,
    [Version] nvarchar(20) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsMandatory] bit NOT NULL,
    [AcceptedBy] nvarchar(max) NULL,
    [AcceptedDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_TermsAndConditions] PRIMARY KEY ([TermsId]),
    CONSTRAINT [FK_TermsAndConditions_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [AppUsers] (
    [UserId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [RoleId] int NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [UserName] nvarchar(100) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [PasswordHash] varbinary(max) NOT NULL,
    [Salt] varbinary(max) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [IsActive] bit NOT NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [LastLoginDate] datetime2 NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AppUsers] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_AppUsers_AppRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AppRoles] ([RoleId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AppUsers_AppUsers_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AppUsers_AppUsers_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AppUsers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [SubscriptionFeatures] (
    [SubscriptionFeatureId] int NOT NULL IDENTITY,
    [PlanId] int NOT NULL,
    [FeatureKey] nvarchar(100) NOT NULL,
    [FeatureValue] nvarchar(max) NULL,
    [IsEnabled] bit NOT NULL,
    CONSTRAINT [PK_SubscriptionFeatures] PRIMARY KEY ([SubscriptionFeatureId]),
    CONSTRAINT [FK_SubscriptionFeatures_SubscriptionPlans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [SubscriptionPlans] ([PlanId]) ON DELETE CASCADE
);

CREATE TABLE [TenantSubscriptions] (
    [TenantSubscriptionId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [PlanId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [PaymentStatus] nvarchar(20) NOT NULL,
    [PaymentMethod] nvarchar(max) NULL,
    [TransactionId] nvarchar(max) NULL,
    [AmountPaid] decimal(18,2) NULL,
    [PaymentDate] datetime2 NULL,
    [AutoRenew] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_TenantSubscriptions] PRIMARY KEY ([TenantSubscriptionId]),
    CONSTRAINT [FK_TenantSubscriptions_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TenantSubscriptions_SubscriptionPlans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [SubscriptionPlans] ([PlanId]) ON DELETE NO ACTION
);

CREATE TABLE [AuditLogs] (
    [AuditId] bigint NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [UserId] int NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [TableName] nvarchar(max) NULL,
    [RecordId] nvarchar(max) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([AuditId]),
    CONSTRAINT [FK_AuditLogs_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AuditLogs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [Branches] (
    [BranchId] int NOT NULL IDENTITY,
    [CompanyId] int NOT NULL,
    [BranchCode] nvarchar(20) NOT NULL,
    [BranchName] nvarchar(200) NOT NULL,
    [AddressLine1] nvarchar(200) NOT NULL,
    [AddressLine2] nvarchar(max) NULL,
    [City] nvarchar(100) NOT NULL,
    [State] nvarchar(50) NOT NULL,
    [PostalCode] nvarchar(20) NOT NULL,
    [Country] nvarchar(50) NOT NULL,
    [Phone] nvarchar(20) NOT NULL,
    [Email] nvarchar(max) NULL,
    [Latitude] decimal(18,2) NULL,
    [Longitude] decimal(18,2) NULL,
    [ManagerId] int NULL,
    [IsActive] bit NOT NULL,
    [OperatingHoursJSON] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY ([BranchId]),
    CONSTRAINT [FK_Branches_AppUsers_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Branches_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE NO ACTION
);

CREATE TABLE [UserTermsAcceptances] (
    [UserTermsAcceptanceId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [TermsId] int NOT NULL,
    [AcceptedAt] datetime2 NOT NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    CONSTRAINT [PK_UserTermsAcceptances] PRIMARY KEY ([UserTermsAcceptanceId]),
    CONSTRAINT [FK_UserTermsAcceptances_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserTermsAcceptances_TermsAndConditions_TermsId] FOREIGN KEY ([TermsId]) REFERENCES [TermsAndConditions] ([TermsId]) ON DELETE NO ACTION
);

CREATE TABLE [UserBranches] (
    [UserBranchId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [BranchId] int NOT NULL,
    [IsPrimary] bit NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserBranches] PRIMARY KEY ([UserBranchId]),
    CONSTRAINT [FK_UserBranches_AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUsers] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserBranches_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([BranchId]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_AppRoles_RoleName] ON [AppRoles] ([RoleName]);

CREATE INDEX [IX_AppUsers_CompanyId] ON [AppUsers] ([CompanyId]);

CREATE INDEX [IX_AppUsers_CreatedBy] ON [AppUsers] ([CreatedBy]);

CREATE UNIQUE INDEX [IX_AppUsers_Email] ON [AppUsers] ([Email]);

CREATE INDEX [IX_AppUsers_RoleId] ON [AppUsers] ([RoleId]);

CREATE INDEX [IX_AppUsers_UpdatedBy] ON [AppUsers] ([UpdatedBy]);

CREATE INDEX [IX_AuditLogs_CompanyId] ON [AuditLogs] ([CompanyId]);

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);

CREATE UNIQUE INDEX [IX_Branches_BranchCode] ON [Branches] ([BranchCode]);

CREATE INDEX [IX_Branches_CompanyId] ON [Branches] ([CompanyId]);

CREATE INDEX [IX_Branches_ManagerId] ON [Branches] ([ManagerId]);

CREATE UNIQUE INDEX [IX_Devices_CompanyId_DeviceCode] ON [Devices] ([CompanyId], [DeviceCode]);

CREATE INDEX [IX_SubscriptionFeatures_PlanId] ON [SubscriptionFeatures] ([PlanId]);

CREATE UNIQUE INDEX [IX_SubscriptionPlans_PlanCode] ON [SubscriptionPlans] ([PlanCode]);

CREATE UNIQUE INDEX [IX_SubscriptionPlans_PlanName] ON [SubscriptionPlans] ([PlanName]);

CREATE INDEX [IX_TenantSubscriptions_CompanyId] ON [TenantSubscriptions] ([CompanyId]);

CREATE INDEX [IX_TenantSubscriptions_PlanId] ON [TenantSubscriptions] ([PlanId]);

CREATE INDEX [IX_TermsAndConditions_CompanyId] ON [TermsAndConditions] ([CompanyId]);

CREATE INDEX [IX_UserBranches_BranchId] ON [UserBranches] ([BranchId]);

CREATE INDEX [IX_UserBranches_UserId] ON [UserBranches] ([UserId]);

CREATE INDEX [IX_UserTermsAcceptances_TermsId] ON [UserTermsAcceptances] ([TermsId]);

CREATE INDEX [IX_UserTermsAcceptances_UserId] ON [UserTermsAcceptances] ([UserId]);

ALTER TABLE [AspNetUsers] ADD CONSTRAINT [FK_AspNetUsers_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([CompanyId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260911131504_AddDeviceToMasterErp', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [CompanyDatabases] ADD [CredentialKey] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260911132727_AddCredentialKeyToCompanyDatabase', N'10.0.0');

COMMIT;
GO

