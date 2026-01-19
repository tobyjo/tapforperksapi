using System.ComponentModel.DataAnnotations;

namespace SaveForPerksAPI.Models
{
    public class ScanEventForCreationDto
    {
        [Required(ErrorMessage = "Reward ID is required")]
        public Guid RewardId { get; set; }

        [Required(ErrorMessage = "RewardOwnerUserId is required")]
        public Guid? RewardOwnerUserId { get; set; }

        [Required(ErrorMessage = "QR Code value is required")]
        public string QrCodeValue { get; set; } = null!;

        [Required(ErrorMessage = "Points change is required")]
        [Range(1, 10, ErrorMessage = "Points change must be at least 1")]
        public int PointsChange { get; set; }   // Bought more than 1 coffee

        [Range(0, 10, ErrorMessage = "NumRewardsToClaim must be no more than 10")]
        public int NumRewardsToClaim { get; set; }
    }
}
