namespace ElsakaHerbs.DataAccess.Entities;

public class Package : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int PacketsCount { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal Price { get; set; }
    public int PurchaseCount { get; set; } = 0;
    public string WeightGrams { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<PackageImage> Images { get; set; } = new List<PackageImage>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
