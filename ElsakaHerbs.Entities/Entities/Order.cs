namespace ElsakaHerbs.DataAccess.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerFirstName { get; set; } = string.Empty;
    public string CustomerLastName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int GovernorateShippingId { get; set; }
    public string GovernorateNameSnapshot { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal GrandTotal { get; set; }
    public int OrderStatusId { get; set; } = 1;
    public DateTime CreatedAt { get; set; }

    public GovernorateShipping GovernorateShipping { get; set; } = null!;
    public OrderStatus OrderStatus { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
