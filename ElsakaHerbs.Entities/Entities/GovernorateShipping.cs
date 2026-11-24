namespace ElsakaHerbs.DataAccess.Entities;

public class GovernorateShipping : BaseEntity
{
    public string GovernorateKey { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
