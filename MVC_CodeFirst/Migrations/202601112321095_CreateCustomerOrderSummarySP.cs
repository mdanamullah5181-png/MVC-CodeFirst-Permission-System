namespace MVC_CodeFirst.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateCustomerOrderSummarySP : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                CREATE PROCEDURE GetCustomerOrderSummary
                    @CustomerId INT
                AS
                BEGIN
                    -- 1. Customer Information 
                    SELECT 
                        c.CustomerId,
                        c.CustomerName,
                        c.ContactNumber,
                        c.ContactAddress,
                        COUNT(DISTINCT o.OrderId) as TotalOrders,
                        SUM(o.TotalAmount) as TotalSpent,
                        MAX(o.OrderDate) as LastOrderDate
                    FROM Customers c
                    LEFT JOIN Orders o ON c.CustomerId = o.CustomerId
                    WHERE c.CustomerId = @CustomerId
                    GROUP BY c.CustomerId, c.CustomerName, c.ContactNumber, c.ContactAddress
                    
                    -- 2. Orders List 
                    SELECT 
                        o.OrderId,
                        o.OrderDate,
                        o.TotalAmount,
                        COUNT(od.OrderDetailId) as NumberOfItems
                    FROM Orders o
                    LEFT JOIN OrderDetails od ON o.OrderId = od.OrderId
                    WHERE o.CustomerId = @CustomerId
                    GROUP BY o.OrderId, o.OrderDate, o.TotalAmount
                    ORDER BY o.OrderDate DESC
                    
                    -- 3. Order Details 
                    SELECT 
                        od.OrderId,
                        od.OrderDetailId,
                        p.ProductName,
                        pc.CategoryName,
                        od.OrderQuantity,
                        od.OrderUnit,
                        od.UnitPrice,
                        od.Amount,
                        p.AvailableQuantity as CurrentStock
                    FROM OrderDetails od
                    INNER JOIN Products p ON od.ProductId = p.ProductId
                    INNER JOIN ProductCategories pc ON od.ProductCategoryId = pc.ProductCategoryId
                    INNER JOIN Orders o ON od.OrderId = o.OrderId
                    WHERE o.CustomerId = @CustomerId
                    ORDER BY o.OrderDate DESC, od.OrderDetailId
                END
            ");
        }
        
        public override void Down()
        {
            Sql("DROP PROCEDURE IF EXISTS GetCustomerOrderSummary");
        }
    }
}
