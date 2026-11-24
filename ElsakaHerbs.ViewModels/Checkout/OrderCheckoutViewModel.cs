using System.ComponentModel.DataAnnotations;

namespace ElsakaHerbs.ViewModels.Checkout;

public class OrderCheckoutViewModel
{
    [Required(ErrorMessage = "الاسم الأول مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم الأول يجب ألا يتجاوز 100 حرف")]
    public string CustomerFirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم العائلة مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم العائلة يجب ألا يتجاوز 100 حرف")]
    public string CustomerLastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [MaxLength(30, ErrorMessage = "رقم الهاتف غير صالح")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى اختيار المحافظة")]
    public int GovernorateShippingId { get; set; }

    [Required(ErrorMessage = "العنوان بالتفصيل مطلوب")]
    public string Address { get; set; } = string.Empty;

    public string? Notes { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "السلة فارغة")]
    public List<OrderItemInputModel> Items { get; set; } = new();
}

public class OrderItemInputModel
{
    [Required]
    public int PackageId { get; set; }

    [Range(1, 99, ErrorMessage = "الكمية يجب أن تكون بين 1 و 99")]
    public int Quantity { get; set; }
}
