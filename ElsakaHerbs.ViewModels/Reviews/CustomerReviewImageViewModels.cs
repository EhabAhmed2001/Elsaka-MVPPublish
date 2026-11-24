namespace ElsakaHerbs.ViewModels.Reviews;

public class CustomerReviewImageViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}
