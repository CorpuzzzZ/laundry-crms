SET NOCOUNT ON;

DECLARE @numOrders INT = 200;
DECLARE @daysBack INT = 30;
DECLARE @basePrice DECIMAL(18,2) = 160.00;
DECLARE @customerMin INT = 1;
DECLARE @customerMax INT = 61;
DECLARE @serviceId INT = 13;
DECLARE @counter INT = 1;
DECLARE @nextOrderNum INT;

SELECT @nextOrderNum = ISNULL(MAX(CAST(RIGHT(OrderNumber, 4) AS INT)), 0) + 1
FROM Orders
WHERE OrderNumber LIKE 'ORD-2026-%';

WHILE @counter <= @numOrders
BEGIN
    DECLARE @custId INT = @customerMin + ABS(CHECKSUM(NEWID())) % (@customerMax - @customerMin + 1);
    DECLARE @weight DECIMAL(10,2) = CAST(0.5 + (ABS(CHECKSUM(NEWID())) % 750) / 100.0 AS DECIMAL(10,2));
    DECLARE @dayOffset INT = ABS(CHECKSUM(NEWID())) % @daysBack;
    DECLARE @orderDate DATETIME2 = DATEADD(HOUR, -(ABS(CHECKSUM(NEWID())) % 10), DATEADD(DAY, -@dayOffset, SYSUTCDATETIME()));

    DECLARE @subtotal DECIMAL(18,2) = @weight * @basePrice;

    DECLARE @roll INT = ABS(CHECKSUM(NEWID())) % 100;
    DECLARE @statusId INT =
        CASE
            WHEN @roll < 60 THEN 5
            WHEN @roll < 85 THEN 4
            WHEN @roll < 95 THEN 3
            ELSE 1
        END;

    DECLARE @actualPickup DATETIME2 = NULL;
    DECLARE @actualDelivery DATETIME2 = NULL;
    IF @statusId IN (4, 5)
        SET @actualPickup = DATEADD(HOUR, 2 + (ABS(CHECKSUM(NEWID())) % 48), @orderDate);
    IF @statusId = 5
        SET @actualDelivery = DATEADD(HOUR, 24, ISNULL(@actualPickup, @orderDate));

    DECLARE @orderNumber NVARCHAR(50) = 'ORD-2026-' + RIGHT('0000' + CAST(@nextOrderNum AS VARCHAR(10)), 4);

    INSERT INTO Orders (
        OrderNumber, CustomerId, BranchId, OrderType, OrderDate, StatusId, Priority,
        Subtotal, DiscountAmount, TaxAmount, TotalAmount, TotalWeight, TotalItems,
        CreatedAt, ActualPickupDate, ActualDeliveryDate
    )
    VALUES (
        @orderNumber, @custId, 1, 'WalkIn', @orderDate, @statusId, 'Normal',
        @subtotal, 0, 0, @subtotal, @weight, 1,
        @orderDate, @actualPickup, @actualDelivery
    );

    DECLARE @orderId INT = SCOPE_IDENTITY();

    INSERT INTO OrderItems (
        OrderId, ServiceId, GarmentTypeId, Quantity, UnitPrice, DiscountAmount,
        IsCompleted, CompletedAt
    )
    VALUES (
        @orderId, @serviceId, NULL, @weight, @basePrice, 0,
        CASE WHEN @statusId IN (4, 5) THEN 1 ELSE 0 END,
        CASE WHEN @statusId IN (4, 5) THEN ISNULL(@actualPickup, @orderDate) ELSE NULL END
    );

    INSERT INTO OrderStatusHistories (OrderId, StatusId, ChangedAt)
    VALUES (@orderId, 1, @orderDate);

    IF @statusId IN (4, 5)
    BEGIN
        INSERT INTO OrderStatusHistories (OrderId, StatusId, ChangedAt)
        VALUES (@orderId, 4, @actualPickup);
    END

    IF @statusId = 5
    BEGIN
        INSERT INTO OrderStatusHistories (OrderId, StatusId, ChangedAt)
        VALUES (@orderId, 5, @actualDelivery);
    END

    IF @statusId IN (4, 5)
    BEGIN
        DECLARE @methodId INT = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 65 THEN 1 ELSE 2 END;
        DECLARE @payDate DATETIME2 = DATEADD(MINUTE, 30, ISNULL(@actualPickup, @orderDate));
        DECLARE @refNo NVARCHAR(50) = CASE WHEN @methodId = 2 THEN 'GC-' + CAST(ABS(CHECKSUM(NEWID())) % 900000000 + 100000000 AS VARCHAR(20)) ELSE NULL END;

        INSERT INTO OrderPayments (OrderId, PaymentMethodId, Amount, ReferenceNumber, PaymentDate, Status)
        VALUES (@orderId, @methodId, @subtotal, @refNo, @payDate, 'Completed');
    END

    SET @nextOrderNum = @nextOrderNum + 1;
    SET @counter = @counter + 1;
END

SELECT 'Orders created' AS Result, COUNT(*) AS Total FROM Orders WHERE OrderNumber LIKE 'ORD-2026-%';
SELECT 'OrderItems' AS TableName, COUNT(*) AS Total FROM OrderItems;
SELECT 'OrderPayments' AS TableName, COUNT(*) AS Total FROM OrderPayments;
