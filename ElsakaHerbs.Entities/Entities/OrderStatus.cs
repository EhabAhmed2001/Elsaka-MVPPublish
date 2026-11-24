namespace ElsakaHerbs.DataAccess.Entities;

public class OrderStatus : BaseEntity
{
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
