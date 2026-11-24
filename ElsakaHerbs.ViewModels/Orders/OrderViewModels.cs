namespace ElsakaHerbs.ViewModels.Orders;

public class OrderStatusViewModel
{
    public int Id { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}

public class OrderListViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public int ItemsCount { get; set; }
    public int OrderStatusId { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public string OrderStatusColor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OrderDetailViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
    public string CustomerFullName => $"{CustomerFirstName} {CustomerLastName}".Trim();
    public string CustomerPhone { get; set; } = string.Empty;
    public int GovernorateShippingId { get; set; }
    public string GovernorateNameSnapshot { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public int OrderStatusId { get; set; }
    public string OrderStatusName { get; set; } = string.Empty;
    public string OrderStatusColor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDetailViewModel> Items { get; set; } = new();
}

public class OrderItemDetailViewModel
{
    public int Id { get; set; }
    public int? PackageId { get; set; }
    public string PackageNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderStatusUpdateViewModel
{
    public int OrderId { get; set; }
    public int NewStatusId { get; set; }
}

public class OrderStatisticsViewModel
{
    public int TotalOrders { get; set; }
    public int NewOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
}

public class OrderPagedResultViewModel
{
    public List<OrderListViewModel> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 1;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
