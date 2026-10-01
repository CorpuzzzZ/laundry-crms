SET NOCOUNT ON;

DECLARE @numCustomers INT = 50;
DECLARE @counter INT = 1;
DECLARE @nextCode INT;

SELECT @nextCode = ISNULL(MAX(CAST(RIGHT(CustomerCode, 4) AS INT)), 0) + 1
FROM Customers
WHERE CustomerCode LIKE 'CUST-%';

DECLARE @firstNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @firstNames (Name) VALUES
('Maria'),('Jose'),('Ana'),('Juan'),('Rosa'),('Pedro'),('Carmen'),('Miguel'),
('Sofia'),('Carlos'),('Elena'),('Rafael'),('Lucia'),('Diego'),('Beatriz'),
('Manuel'),('Isabel'),('Fernando'),('Pilar'),('Ricardo'),('Teresa'),('Alberto'),
('Gloria'),('Eduardo'),('Cristina'),('Antonio'),('Rosario'),('Sergio'),('Patricia'),
('Alejandro'),('Veronica'),('Ramon'),('Marina'),('Andres'),('Cecilia'),('Joaquin'),
('Natalia'),('Emilio'),('Clara'),('Victor'),('Elena'),('Marcos'),('Adriana'),
('Felipe'),('Camila'),('Hector'),('Silvia'),('Raul'),('Angela'),('Oscar');

DECLARE @lastNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @lastNames (Name) VALUES
('Santos'),('Reyes'),('Cruz'),('Bautista'),('Garcia'),('Mendoza'),('Torres'),
('Flores'),('Ramos'),('Gonzales'),('Villanueva'),('Aquino'),('Castillo'),
('Domingo'),('Navarro'),('Salazar'),('Pascual'),('Rivera'),('Santiago'),('Valdez'),
('Morales'),('Ramos'),('Aguilar'),('Cortez'),('Ocampo'),('Del Rosario'),('Fernandez'),
('Solis'),('Marquez'),('Dizon'),('Pineda'),('Legaspi'),('Alonzo'),('Rivera'),
('Espiritu'),('Ignacio'),('Lorenzo'),('Roxas'),('Velasco'),('Buenaventura');

DECLARE @cities TABLE (Id INT IDENTITY(1,1), City NVARCHAR(100), State NVARCHAR(100), Postal NVARCHAR(20));
INSERT INTO @cities (City, State, Postal) VALUES
('Manila', 'Metro Manila', '1000'),
('Quezon City', 'Metro Manila', '1100'),
('Makati', 'Metro Manila', '1200'),
('Cebu City', 'Cebu', '6000'),
('Davao City', 'Davao del Sur', '8000'),
('Iloilo City', 'Iloilo', '5000'),
('Bacolod', 'Negros Occidental', '6100'),
('Baguio', 'Benguet', '2600'),
('Cagayan de Oro', 'Misamis Oriental', '9000'),
('Taguig', 'Metro Manila', '1630');

DECLARE @numFirst INT = (SELECT COUNT(*) FROM @firstNames);
DECLARE @numLast  INT = (SELECT COUNT(*) FROM @lastNames);
DECLARE @numCity  INT = (SELECT COUNT(*) FROM @cities);

WHILE @counter <= @numCustomers
BEGIN
    DECLARE @fnIdx INT = (ABS(CHECKSUM(NEWID())) % @numFirst) + 1;
    DECLARE @lnIdx INT = (ABS(CHECKSUM(NEWID())) % @numLast) + 1;
    DECLARE @cityIdx INT = (ABS(CHECKSUM(NEWID())) % @numCity) + 1;

    DECLARE @fn NVARCHAR(50) = (SELECT Name FROM @firstNames WHERE Id = @fnIdx);
    DECLARE @ln NVARCHAR(50) = (SELECT Name FROM @lastNames WHERE Id = @lnIdx);
    DECLARE @city NVARCHAR(100) = (SELECT City FROM @cities WHERE Id = @cityIdx);
    DECLARE @state NVARCHAR(100) = (SELECT State FROM @cities WHERE Id = @cityIdx);
    DECLARE @postal NVARCHAR(20) = (SELECT Postal FROM @cities WHERE Id = @cityIdx);

    DECLARE @daysAgo INT = ABS(CHECKSUM(NEWID())) % 60;
    DECLARE @createdAt DATETIME2 = DATEADD(HOUR, -(ABS(CHECKSUM(NEWID())) % 24), DATEADD(DAY, -@daysAgo, SYSUTCDATETIME()));
    DECLARE @code NVARCHAR(50) = 'CUST-' + RIGHT('0000' + CAST(@nextCode AS VARCHAR(10)), 4);
    DECLARE @phone NVARCHAR(20) = '09' + RIGHT('000000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000000 AS VARCHAR(10)), 9);
    DECLARE @email NVARCHAR(255) = LOWER(@fn) + '.' + LOWER(REPLACE(@ln, ' ', '')) + CAST(@nextCode AS VARCHAR(10)) + '@example.com';

    INSERT INTO Customers (
        CustomerCode, CustomerType, FirstName, LastName, Email, PhonePrimary,
        PreferredLanguage, City, State, PostalCode, Country,
        IsActive, IsArchived, CreatedAt,
        LifetimeSpend, LoyaltyPoints, TotalOrders
    )
    VALUES (
        @code, 'Individual', @fn, @ln, @email, @phone,
        'English', @city, @state, @postal, 'Philippines',
        1, 0, @createdAt,
        0, 0, 0
    );

    SET @nextCode = @nextCode + 1;
    SET @counter = @counter + 1;
END

SELECT COUNT(*) AS TotalCustomers FROM Customers;
SELECT TOP 5 CustomerId, CustomerCode, FirstName, LastName, City, CreatedAt FROM Customers ORDER BY CustomerId DESC;
