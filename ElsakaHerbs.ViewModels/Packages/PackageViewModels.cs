using System.ComponentModel.DataAnnotations;

namespace ElsakaHerbs.ViewModels.Packages;

public class PackageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public int PacketsCount { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal Price { get; set; }
    public int PurchaseCount { get; set; }
    public string WeightGrams { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DefaultImageUrl { get; set; } = string.Empty;
    public List<PackageImageViewModel> Images { get; set; } = new();
}

public class PackageImageViewModel
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class PackageFormModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "اسم العبوة مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم العبوة يجب ألا يتجاوز 200 حرف")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "مدة العبوة بالأيام مطلوبة")]
    [Range(1, int.MaxValue, ErrorMessage = "المدة يجب أن تكون يوماً واحداً على الأقل")]
    public int DurationDays { get; set; }

    [Required(ErrorMessage = "عدد البكتات مطلوب")]
    [Range(1, 1000, ErrorMessage = "عدد البكتات يجب أن يكون 1 على الأقل")]
    public int PacketsCount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "السعر غير صالح")]
    public decimal? OriginalPrice { get; set; }

    [Required(ErrorMessage = "السعر النهائي مطلوب")]
    [Range(0.01, float.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int PurchaseCount { get; set; } = 0;

    public string WeightGrams { get; set; } = string.Empty;

    [Required(ErrorMessage = "الوصف مطلوب")]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Existing images already on the package
    public List<PackageImageViewModel> ExistingImages { get; set; } = new();

    // Deleted image IDs to be removed from existing images
    public List<int>? DeletedImageIds { get; set; }

    // Default image selection (can be ID of existing or index of new image)
    public int? DefaultExistingImageId { get; set; }
    public int? DefaultNewImageIndex { get; set; }
}
