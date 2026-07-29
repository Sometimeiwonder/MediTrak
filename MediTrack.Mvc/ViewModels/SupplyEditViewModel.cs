using System.ComponentModel.DataAnnotations;

namespace MediTrack.Mvc.ViewModels;

public class SupplyEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Mã vật tư không được để trống")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Mã vật tư phải từ 3 đến 50 ký tự")]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Mã vật tư chỉ gồm chữ in hoa, số và dấu -")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Tên vật tư không được để trống")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên vật tư phải từ 3 đến 150 ký tự")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Nhóm vật tư không được để trống")]
    public int SupplyCategoryId { get; set; }

    [Required(ErrorMessage = "Nhà cung cấp không được để trống")]
    public string Supplier { get; set; } = "";

    [Range(1, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "Số lượng tồn không được âm")]
    public int Quantity { get; set; }

    [Range(0, 100000, ErrorMessage = "Mức tối thiểu không được âm")]
    public int MinStock { get; set; }

    public int ConcurrencyVersion { get; set; }

    public string? ImageUrl { get; set; }

    public List<MediTrack.Mvc.Models.SupplyCategory> Categories { get; set; } = new();
}
