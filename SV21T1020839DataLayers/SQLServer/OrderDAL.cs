using DataLayers.SQLServer;
using DataLayers;
using SV21T1020839.DomainModels;
using Dapper;

namespace SV21T1020839.DataLayers.SQLServer
{
    public class OrderDAL : BaseDAL, IOrderDAL<Order>
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {

        }

        public int Add(Order data)
        {
            int id = 0;
            using (var connection = Openconnection())
            {
                var sql = @"insert into Orders(CustomerId, OrderTime,
DeliveryProvince, DeliveryAddress,
EmployeeID, Status)
values(@CustomerID, getdate(),
@DeliveryProvince, @DeliveryAddress,
@EmployeeID, @Status);
select @@identity";

                id = connection.ExecuteScalar<int>(sql, new
                {
                    data.CustomerID,
                    data.DeliveryProvince,
                    data.DeliveryAddress,
                    data.EmployeeID,
                    data.Status
                });
            }
            return id;
        }

        public int Count(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = Openconnection())
            {
                var sql = @"select count(*)
                                from Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                                where (@Status = 0 or o.Status = @Status)
                                and (@FromTime is null or o.OrderTime >= @FromTime)
                                and (@ToTime is null or o.OrderTime <= @ToTime)
                                and (@SearchValue = N'' or c.CustomerName like @SearchValue
                                or e.FullName like @SearchValue
                                or s.ShipperName like @SearchValue)";

                count = connection.ExecuteScalar<int>(sql, new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue
                });
            }
            return count;
        }

        public bool Delete(int orderID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"delete from OrderDetails where OrderID = @OrderID;
delete from Orders where OrderID = @OrderID";
                result = connection.Execute(sql, new { OrderID = orderID }) > 0;
            }
            return result;
        }

        public bool DeleteDetail(int orderID, int productID)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"delete from OrderDetails where OrderID = @OrderID and ProductID = @ProductID";
                result = connection.Execute(sql, new { OrderID = orderID, ProductID = productID }) > 0;
            }
            return result;
        }

        public Order? Get(int orderID)
        {
            Order? data = null;
            using (var connection = Openconnection())
            {
                var sql = @"select o.*, c.CustomerName, c.ContactName as CustomerContactName, c.Address as CustomerAddress,
c.Phone as CustomerPhone, c.Email as CustomerEmail, e.FullName as EmployeeName, s.ShipperName, s.Phone as ShipperPhone
from Orders as o
left join Customers as c on o.CustomerID = c.CustomerID
left join Employees as e on o.EmployeeID = e.EmployeeID
left join Shippers as s on o.ShipperID = s.ShipperID
where o.OrderID = @OrderID";

                data = connection.QuerySingleOrDefault<Order>(sql, new { OrderID = orderID });
            }
            return data;
        }

        public OrderDetail? GetDetail(int orderID, int productID)
        {
            OrderDetail? data = null;
            using (var connection = Openconnection())
            {
                var sql = @"select od.*, p.ProductName, p.Photo, p.Unit
from OrderDetails as od
join Products as p on od.ProductID = p.ProductID
where od.OrderID = @OrderID and od.ProductID = @ProductID";
                data = connection.QuerySingleOrDefault<OrderDetail>(sql, new { OrderID = orderID, ProductID = productID });
            }
            return data;
        }

        public IList<Order> List(int page = 1, int pageSize = 0, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            List<Order> list = new List<Order>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = Openconnection())
            {
                var sql = @"with cte as
(
select row_number() over(order by o.OrderTime desc) as RowNumber,
o.*, c.CustomerName, c.ContactName as CustomerContactName, c.Address as CustomerAddress,
c.Phone as CustomerPhone, c.Email as CustomerEmail, e.FullName as EmployeeName, s.ShipperName, s.Phone as ShipperPhone
from Orders as o
left join Customers as c on o.CustomerID = c.CustomerID
left join Employees as e on o.EmployeeID = e.EmployeeID
left join Shippers as s on o.ShipperID = s.ShipperID
where (@Status = 0 or o.Status = @Status)
and (@FromTime is null or o.OrderTime >= @FromTime)
and (@ToTime is null or o.OrderTime <= @ToTime)
and (@SearchValue = N'' or c.CustomerName like @SearchValue
or e.FullName like @SearchValue
or s.ShipperName like @SearchValue)
)
select * from cte
where (@PageSize = 0)
or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)
order by RowNumber";

                list = connection.Query<Order>(sql, new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue,
                    Page = page,
                    PageSize = pageSize
                }).ToList();
            }
            return list;
        }

        public IList<OrderDetail> ListDetails(int orderID)
        {
            List<OrderDetail> list = new List<OrderDetail>();
            using (var connection = Openconnection())
            {
                var sql = @"select od.*, p.ProductName, p.Photo, p.Unit
from OrderDetails as od
join Products as p on od.ProductID = p.ProductID
where od.OrderID = @OrderID";
                list = connection.Query<OrderDetail>(sql, new { OrderID = orderID }).ToList();
            }
            return list;
        }

        public bool SaveDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"if exists(select * from OrderDetails where OrderID = @OrderID and ProductID = @ProductID)
update OrderDetails
set Quantity = @Quantity, SalePrice = @SalePrice
where OrderID = @OrderID and ProductID = @ProductID
else
insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice)
values(@OrderID, @ProductID, @Quantity, @SalePrice)";

                result = connection.Execute(sql, new
                {
                    OrderID = orderID,
                    ProductID = productID,
                    Quantity = quantity,
                    SalePrice = salePrice
                }) > 0;
            }
            return result;
        }

        public bool Update(Order data)
        {
            bool result = false;
            using (var connection = Openconnection())
            {
                var sql = @"update Orders
set CustomerID = @CustomerID,
OrderTime = @OrderTime,
DeliveryProvince = @DeliveryProvince,
DeliveryAddress = @DeliveryAddress,
EmployeeID = @EmployeeID,
AcceptTime = @AcceptTime,
ShipperID = @ShipperID,
ShippedTime = @ShippedTime,
FinishedTime = @FinishedTime,
Status = @Status
where OrderID = @OrderID";

                result = connection.Execute(sql, new
                {
                    data.CustomerID,
                    data.OrderTime,
                    data.DeliveryProvince,
                    data.DeliveryAddress,
                    data.EmployeeID,
                    data.AcceptTime,
                    data.ShipperID,
                    data.ShippedTime,
                    data.FinishedTime,
                    data.Status,
                    data.OrderID
                }) > 0;
            }
            return result;
        }
    }
}
