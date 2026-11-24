using System.ComponentModel.DataAnnotations;

namespace ElsakaHerbs.ViewModels.SiteContent;

public class SiteContentViewModel
{
    public int Id { get; set; }
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroHighlight { get; set; } = string.Empty;
    public string CtaEyebrow { get; set; } = string.Empty;
    public string CtaTitle { get; set; } = string.Empty;
    public string CtaDescription { get; set; } = string.Empty;
    public string WhatsAppNumber { get; set; } = string.Empty;
    public bool IsWebsiteEnabled { get; set; } = true;
    public string? MaintenanceMessage { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SiteContentEditViewModel
{
    [Required(ErrorMessage = "العنوان الرئيسي مطلوب")]
    [MaxLength(200, ErrorMessage = "العنوان الرئيسي يجب ألا يتجاوز 200 حرف")]
    public string HeroTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "الجزء المميز مطلوب")]
    [MaxLength(200, ErrorMessage = "الجزء المميز يجب ألا يتجاوز 200 حرف")]
    public string HeroHighlight { get; set; } = string.Empty;

    [Required(ErrorMessage = "النص الصغير مطلوب")]
    [MaxLength(150, ErrorMessage = "النص الصغير يجب ألا يتجاوز 150 حرف")]
    public string CtaEyebrow { get; set; } = string.Empty;

    [Required(ErrorMessage = "عنوان قسم الطلب مطلوب")]
    [MaxLength(200, ErrorMessage = "العنوان يجب ألا يتجاوز 200 حرف")]
    public string CtaTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "وصف قسم الطلب مطلوب")]
    public string CtaDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم واتساب مطلوب")]
    [MaxLength(30, ErrorMessage = "رقم واتساب غير صالح")]
    public string WhatsAppNumber { get; set; } = string.Empty;

    public bool IsWebsiteEnabled { get; set; } = true;

    [MaxLength(1000, ErrorMessage = "رسالة الصيانة يجب ألا تتجاوز 1000 حرف")]
    public string? MaintenanceMessage { get; set; }
}
