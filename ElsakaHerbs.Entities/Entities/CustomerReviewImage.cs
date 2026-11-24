namespace ElsakaHerbs.DataAccess.Entities;

public class CustomerReviewImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
