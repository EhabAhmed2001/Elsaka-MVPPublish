using System.ComponentModel.DataAnnotations;

namespace ElsakaHerbs.ViewModels.Shipping;

public class GovernorateShippingViewModel
{
    public int Id { get; set; }
    public string GovernorateKey { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public decimal ShippingCost { get; set; }
    public bool IsActive { get; set; }
}

public class GovernorateShippingEditModel
{
    public int Id { get; set; }

    [Required]
    public string GovernorateKey { get; set; } = string.Empty;

    [Range(0, 10000, ErrorMessage = "مصاريف الشحن يجب ألا تقل عن صفر")]
    public decimal ShippingCost { get; set; }

    public bool IsActive { get; set; }
}
