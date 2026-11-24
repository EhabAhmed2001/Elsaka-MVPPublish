namespace ElsakaHerbs.ViewModels.Checkout;

public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemSummaryViewModel> Items { get; set; } = new();
}

public class OrderItemSummaryViewModel
{
    public string PackageName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
