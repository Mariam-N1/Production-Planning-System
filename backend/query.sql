-- ============================================================
--  CrudDb · Products · a query lesson you can run
--  Highlight ONE query, press Cmd+Enter (DBeaver)
--  or Cmd+Shift+E (VS Code) to run just that one.
-- ============================================================

USE CrudDb;


-- ============================================================
--  1. SELECT — which columns do you want?
-- ============================================================

-- * means "every column"
SELECT * FROM Products;

-- Or name the ones you want, in any order
-- SELECT Name, Price FROM Products;

-- Rename a column in the output with AS
-- SELECT Name AS Product, Price AS Cost FROM Products;

-- Columns can be calculated. This one isn't stored anywhere.
-- SELECT Name, Price, Quantity, Price * Quantity AS StockValue FROM Products;

-- Only the first 3 rows
-- SELECT TOP 3 * FROM Products;


-- ============================================================
--  2. WHERE — which rows do you want?
-- ============================================================

-- Exact match on a number
-- SELECT * FROM Products WHERE Id = 7;

-- Exact match on text (single quotes, always)
-- SELECT * FROM Products WHERE Name = 'laptop';

-- Comparisons
-- SELECT * FROM Products WHERE Price > 100;
-- SELECT * FROM Products WHERE Quantity <= 5;
-- SELECT * FROM Products WHERE Price <> 570;      -- <> means "not equal"

-- Two conditions, both must be true
-- SELECT * FROM Products WHERE Price > 100 AND Quantity > 2;

-- Two conditions, either will do
-- SELECT * FROM Products WHERE Price < 50 OR Quantity > 8;

-- A range
-- SELECT * FROM Products WHERE Price BETWEEN 100 AND 600;

-- A list of options
-- SELECT * FROM Products WHERE Name IN ('laptop', 'Mouse', 'Keyboard');

-- Partial text match. % means "anything can go here"
-- SELECT * FROM Products WHERE Name LIKE 'Mac%';    -- starts with Mac
-- SELECT * FROM Products WHERE Name LIKE '%book';   -- ends with book
-- SELECT * FROM Products WHERE Name LIKE '%boo%';   -- contains boo

-- Empty values
-- SELECT * FROM Products WHERE Name IS NULL;
-- SELECT * FROM Products WHERE Name IS NOT NULL;


-- ============================================================
--  3. ORDER BY — what order?
-- ============================================================

-- SELECT * FROM Products ORDER BY Price;           -- cheapest first (default)
-- SELECT * FROM Products ORDER BY Price DESC;      -- dearest first
-- SELECT * FROM Products ORDER BY Name ASC;        -- A to Z

-- Sort by one thing, then another
-- SELECT * FROM Products ORDER BY Quantity DESC, Price ASC;

-- The 3 most expensive products
-- SELECT TOP 3 * FROM Products ORDER BY Price DESC;


-- ============================================================
--  4. AGGREGATES — one answer from many rows
-- ============================================================

-- SELECT COUNT(*) AS HowMany FROM Products;
-- SELECT SUM(Quantity) AS TotalItems FROM Products;
-- SELECT AVG(Price) AS AveragePrice FROM Products;
-- SELECT MIN(Price) AS Cheapest, MAX(Price) AS Dearest FROM Products;

-- What is all your stock worth?
-- SELECT SUM(Price * Quantity) AS TotalStockValue FROM Products;

-- Aggregates respect WHERE
-- SELECT COUNT(*) AS ExpensiveOnes FROM Products WHERE Price > 100;

-- Several answers at once
-- SELECT
--     COUNT(*)              AS Products,
--     SUM(Quantity)         AS Items,
--     AVG(Price)            AS AvgPrice,
--     SUM(Price * Quantity) AS StockValue
-- FROM Products;


-- ============================================================
--  5. GROUP BY — one answer PER GROUP
-- ============================================================

-- How many products share each name?
-- SELECT Name, COUNT(*) AS HowMany
-- FROM Products
-- GROUP BY Name;

-- Total stock value per product name, dearest group first
-- SELECT Name, SUM(Price * Quantity) AS Value
-- FROM Products
-- GROUP BY Name
-- ORDER BY Value DESC;

-- HAVING filters GROUPS. WHERE filters ROWS. That's the whole difference.
-- SELECT Name, COUNT(*) AS HowMany
-- FROM Products
-- GROUP BY Name
-- HAVING COUNT(*) > 1;


-- ============================================================
--  6. LOOKING AT THE DATABASE ITSELF
-- ============================================================

-- What tables are in here?
-- SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;

-- What columns does Products have, and what types?
-- SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'Products';

-- Which EF Core migrations have been applied?
-- SELECT * FROM __EFMigrationsHistory;


-- ============================================================
--  7. WHAT YOUR API DOES UNDERNEATH
--  Read these. Running them bypasses your API's rules -
--  fine here while learning, not a habit to keep.
-- ============================================================

-- Your POST endpoint generates roughly:
-- INSERT INTO Products (Name, Price, Quantity) VALUES ('Test', 9.99, 1);

-- Your PUT endpoint generates roughly:
-- UPDATE Products SET Name = 'Test2', Price = 12.50, Quantity = 3 WHERE Id = 7;

-- Your DELETE endpoint generates roughly:
-- DELETE FROM Products WHERE Id = 7;

-- ALWAYS write the WHERE first on UPDATE and DELETE.
-- Without it, you change or delete EVERY row. There is no undo.
