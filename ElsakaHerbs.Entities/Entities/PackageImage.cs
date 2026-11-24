namespace ElsakaHerbs.DataAccess.Entities;

public class PackageImage : BaseEntity
{
    public int PackageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    public Package Package { get; set; } = null!;
}
