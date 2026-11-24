namespace ElsakaHerbs.DataAccess.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int? PackageId { get; set; }
    public string PackageNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public Order Order { get; set; } = null!;
    public Package? Package { get; set; }
}
