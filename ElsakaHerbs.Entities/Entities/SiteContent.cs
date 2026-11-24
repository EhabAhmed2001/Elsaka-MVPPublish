namespace ElsakaHerbs.DataAccess.Entities;

public class SiteContent : BaseEntity
{
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
