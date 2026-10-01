SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

-- 1. Ensure Roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SuperAdmin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Manager')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (NEWID(), 'Manager', 'MANAGER', NEWID());

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Crew')
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp) VALUES (NEWID(), 'Crew', 'CREW', NEWID());

-- 2. Ensure Companies
IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyCode = 'CRM001')
    INSERT INTO Companies (CompanyCode, CompanyName, LegalName, TaxId, RegistrationNumber, Website, Industry, IsActive, CreatedAt, CreatedBy)
    VALUES ('CRM001', 'CRM Solutions Inc.', 'CRM Solutions Inc.', '123-456-789', 'REG-2026-001', 'https://crmsolutions.com', 'Laundry Services', 1, SYSUTCDATETIME(), 'System');

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyCode = 'CRM002')
    INSERT INTO Companies (CompanyCode, CompanyName, LegalName, TaxId, RegistrationNumber, Website, Industry, IsActive, CreatedAt, CreatedBy)
    VALUES ('CRM002', 'Fresh & Clean Laundromat', 'Fresh & Clean Laundromat Ltd.', '234-567-890', 'REG-2026-002', 'https://freshclean.com', 'Laundry Services', 1, SYSUTCDATETIME(), 'System');

IF NOT EXISTS (SELECT 1 FROM Companies WHERE CompanyCode = 'CRM003')
    INSERT INTO Companies (CompanyCode, CompanyName, LegalName, TaxId, RegistrationNumber, Website, Industry, IsActive, CreatedAt, CreatedBy)
    VALUES ('CRM003', 'QuickWash Express', 'QuickWash Express Co.', '345-678-901', 'REG-2026-003', 'https://quickwash.com', 'Laundry Services', 1, SYSUTCDATETIME(), 'System');

DECLARE @Comp1Id INT = (SELECT TOP 1 CompanyId FROM Companies WHERE CompanyCode = 'CRM001');
DECLARE @Comp2Id INT = (SELECT TOP 1 CompanyId FROM Companies WHERE CompanyCode = 'CRM002');
DECLARE @Comp3Id INT = (SELECT TOP 1 CompanyId FROM Companies WHERE CompanyCode = 'CRM003');

-- 3. Configure Cloud CompanyDatabases Catalog Routing
-- TenantDB1
IF EXISTS (SELECT 1 FROM CompanyDatabases WHERE CompanyId = @Comp1Id)
BEGIN
    UPDATE CompanyDatabases
    SET ServerName = 'db70930.public.databaseasp.net,1433',
        DatabaseName = 'db70930',
        CredentialKey = 'TenantDB1',
        IsActive = 1
    WHERE CompanyId = @Comp1Id;
END
ELSE
BEGIN
    INSERT INTO CompanyDatabases (CompanyId, ServerName, DatabaseName, CredentialKey, IsActive, CreatedAt)
    VALUES (@Comp1Id, 'db70930.public.databaseasp.net,1433', 'db70930', 'TenantDB1', 1, SYSUTCDATETIME());
END

-- TenantDB2
IF EXISTS (SELECT 1 FROM CompanyDatabases WHERE CompanyId = @Comp2Id)
BEGIN
    UPDATE CompanyDatabases
    SET ServerName = 'db70932.public.databaseasp.net,1433',
        DatabaseName = 'db70932',
        CredentialKey = 'TenantDB2',
        IsActive = 1
    WHERE CompanyId = @Comp2Id;
END
ELSE
BEGIN
    INSERT INTO CompanyDatabases (CompanyId, ServerName, DatabaseName, CredentialKey, IsActive, CreatedAt)
    VALUES (@Comp2Id, 'db70932.public.databaseasp.net,1433', 'db70932', 'TenantDB2', 1, SYSUTCDATETIME());
END

-- TenantDB3
IF EXISTS (SELECT 1 FROM CompanyDatabases WHERE CompanyId = @Comp3Id)
BEGIN
    UPDATE CompanyDatabases
    SET ServerName = 'db70933.public.databaseasp.net,1433',
        DatabaseName = 'db70933',
        CredentialKey = 'TenantDB3',
        IsActive = 1
    WHERE CompanyId = @Comp3Id;
END
ELSE
BEGIN
    INSERT INTO CompanyDatabases (CompanyId, ServerName, DatabaseName, CredentialKey, IsActive, CreatedAt)
    VALUES (@Comp3Id, 'db70933.public.databaseasp.net,1433', 'db70933', 'TenantDB3', 1, SYSUTCDATETIME());
END

-- 4. Subscription Plans
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans WHERE PlanCode = 'PLAN1')
    INSERT INTO SubscriptionPlans (PlanName, PlanCode, Description, PricePerMonth, PricePerYear, MaxUsers, MaxBranches, MaxOrdersPerMonth, MaxStorageGB, IsActive, SortOrder, CreatedAt)
    VALUES ('Plan 1', 'PLAN1', 'Admin can access Subscription View, Terms and Condition, Reports, Dashboard, Branch Management, Service Management, User Management, Customer Management, Loyalty Management, Order Management.', 4999.00, 49990.00, 999, 999, 10000, 100, 1, 1, SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans WHERE PlanCode = 'PLAN2')
    INSERT INTO SubscriptionPlans (PlanName, PlanCode, Description, PricePerMonth, PricePerYear, MaxUsers, MaxBranches, MaxOrdersPerMonth, MaxStorageGB, IsActive, SortOrder, CreatedAt)
    VALUES ('Plan 2', 'PLAN2', 'Admin can access Subscription View, Terms and Condition, Dashboard, Branch - only 1 branch, Service Management, User Management - only 1 manager and 1 crew, Customer Management, Loyalty Management, Order Management.', 2499.00, 24990.00, 3, 1, 2000, 20, 1, 2, SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans WHERE PlanCode = 'PLAN3')
    INSERT INTO SubscriptionPlans (PlanName, PlanCode, Description, PricePerMonth, PricePerYear, MaxUsers, MaxBranches, MaxOrdersPerMonth, MaxStorageGB, IsActive, SortOrder, CreatedAt)
    VALUES ('Plan 3', 'PLAN3', 'Admin can access Subscription View, Terms and Condition, Dashboard, Report, Service Management, User Management, Customer Management, Order Management.', 1999.00, 19990.00, 10, 0, 5000, 50, 1, 3, SYSUTCDATETIME());

DECLARE @Plan1Id INT = (SELECT TOP 1 PlanId FROM SubscriptionPlans WHERE PlanCode = 'PLAN1');
DECLARE @Plan2Id INT = (SELECT TOP 1 PlanId FROM SubscriptionPlans WHERE PlanCode = 'PLAN2');
DECLARE @Plan3Id INT = (SELECT TOP 1 PlanId FROM SubscriptionPlans WHERE PlanCode = 'PLAN3');

-- 5. Tenant Subscriptions
IF @Comp1Id IS NOT NULL AND @Plan1Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM TenantSubscriptions WHERE CompanyId = @Comp1Id)
    INSERT INTO TenantSubscriptions (CompanyId, PlanId, StartDate, EndDate, IsActive, PaymentStatus, PaymentMethod, AmountPaid, AutoRenew, CreatedAt)
    VALUES (@Comp1Id, @Plan1Id, SYSUTCDATETIME(), DATEADD(year, 1, SYSUTCDATETIME()), 1, 'Paid', 'System Seed', 49990.00, 1, SYSUTCDATETIME());

IF @Comp2Id IS NOT NULL AND @Plan2Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM TenantSubscriptions WHERE CompanyId = @Comp2Id)
    INSERT INTO TenantSubscriptions (CompanyId, PlanId, StartDate, EndDate, IsActive, PaymentStatus, PaymentMethod, AmountPaid, AutoRenew, CreatedAt)
    VALUES (@Comp2Id, @Plan2Id, SYSUTCDATETIME(), DATEADD(year, 1, SYSUTCDATETIME()), 1, 'Paid', 'System Seed', 24990.00, 1, SYSUTCDATETIME());

IF @Comp3Id IS NOT NULL AND @Plan3Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM TenantSubscriptions WHERE CompanyId = @Comp3Id)
    INSERT INTO TenantSubscriptions (CompanyId, PlanId, StartDate, EndDate, IsActive, PaymentStatus, PaymentMethod, AmountPaid, AutoRenew, CreatedAt)
    VALUES (@Comp3Id, @Plan3Id, SYSUTCDATETIME(), DATEADD(year, 1, SYSUTCDATETIME()), 1, 'Paid', 'System Seed', 19990.00, 1, SYSUTCDATETIME());

-- 6. Terms & Conditions
IF NOT EXISTS (SELECT 1 FROM TermsAndConditions WHERE Version = 'v1.0')
    INSERT INTO TermsAndConditions (Version, Title, Content, EffectiveDate, IsActive, IsMandatory, CreatedAt)
    VALUES ('v1.0', 'Standard Laundry CRM Platform Terms of Service', '# Platform Terms of Service
## 1. Acceptance of Terms
By accessing or using the Laundry CRM system, all tenant organizations, administrators, and authorized users agree to comply with and be bound by these Terms of Service.
## 2. Permitted Use
The CRM platform is provided solely for lawful laundry business management, customer relationship handling, order tracking, and inventory operations.
## 3. Data Protection and Confidentiality
Tenant business data, customer personally identifiable information (PII), and order records are strictly protected. Each tenant database is isolated.
## 4. Subscription & Payment Terms
Subscription fees are billed in advance on a monthly or annual basis depending on the chosen tier.
## 5. Modifications
Platform administrators reserve the right to revise these Terms upon notifying users of version updates.', SYSUTCDATETIME(), 1, 1, SYSUTCDATETIME());

-- 7. Seed Default SuperAdmin and Admin Users if missing
-- SuperAdmin (Password: Admin@123456)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'superadmin@crm.com')
BEGIN
    DECLARE @SuperAdminId NVARCHAR(450) = NEWID();
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, CompanyId, IsActive, CreatedAt, CreatedBy)
    VALUES (@SuperAdminId, 'superadmin@crm.com', 'SUPERADMIN@CRM.COM', 'superadmin@crm.com', 'SUPERADMIN@CRM.COM', 1, 
            'AQAAAAIAAYagAAAAEIU61pthMM1k8108YNuan78spNn87mHtr+Llfs2MMvVz8QZMkZcBWUUAWKJbe72B/g==', 
            NEWID(), NEWID(), 0, 0, 1, 0, 'Super', 'Admin', @Comp1Id, 1, SYSUTCDATETIME(), 'System');

    DECLARE @SuperAdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'SuperAdmin');
    IF @SuperAdminRoleId IS NOT NULL
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@SuperAdminId, @SuperAdminRoleId);
END

-- Tenant 1 Admin (Password: Admin@123456)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'admin@crm.com')
BEGIN
    DECLARE @Admin1Id NVARCHAR(450) = NEWID();
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, CompanyId, IsActive, CreatedAt, CreatedBy)
    VALUES (@Admin1Id, 'admin@crm.com', 'ADMIN@CRM.COM', 'admin@crm.com', 'ADMIN@CRM.COM', 1, 
            'AQAAAAIAAYagAAAAEIU61pthMM1k8108YNuan78spNn87mHtr+Llfs2MMvVz8QZMkZcBWUUAWKJbe72B/g==', 
            NEWID(), NEWID(), 0, 0, 1, 0, 'Admin', 'One', @Comp1Id, 1, SYSUTCDATETIME(), 'System');

    DECLARE @AdminRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Admin');
    IF @AdminRoleId IS NOT NULL
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@Admin1Id, @AdminRoleId);
END

-- Tenant 1 Manager (Password: Admin@123456)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'manager@crm.com')
BEGIN
    DECLARE @Manager1Id NVARCHAR(450) = NEWID();
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, CompanyId, IsActive, CreatedAt, CreatedBy)
    VALUES (@Manager1Id, 'manager@crm.com', 'MANAGER@CRM.COM', 'manager@crm.com', 'MANAGER@CRM.COM', 1, 
            'AQAAAAIAAYagAAAAEIU61pthMM1k8108YNuan78spNn87mHtr+Llfs2MMvVz8QZMkZcBWUUAWKJbe72B/g==', 
            NEWID(), NEWID(), 0, 0, 1, 0, 'Manager', 'One', @Comp1Id, 1, SYSUTCDATETIME(), 'System');

    DECLARE @ManagerRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Manager');
    IF @ManagerRoleId IS NOT NULL
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@Manager1Id, @ManagerRoleId);
END

-- Tenant 1 Crew (Password: Admin@123456)
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'crew@crm.com')
BEGIN
    DECLARE @Crew1Id NVARCHAR(450) = NEWID();
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName, CompanyId, IsActive, CreatedAt, CreatedBy)
    VALUES (@Crew1Id, 'crew@crm.com', 'CREW@CRM.COM', 'crew@crm.com', 'CREW@CRM.COM', 1, 
            'AQAAAAIAAYagAAAAEIU61pthMM1k8108YNuan78spNn87mHtr+Llfs2MMvVz8QZMkZcBWUUAWKJbe72B/g==', 
            NEWID(), NEWID(), 0, 0, 1, 0, 'Crew', 'One', @Comp1Id, 1, SYSUTCDATETIME(), 'System');

    DECLARE @CrewRoleId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetRoles WHERE Name = 'Crew');
    IF @CrewRoleId IS NOT NULL
        INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@Crew1Id, @CrewRoleId);
END
