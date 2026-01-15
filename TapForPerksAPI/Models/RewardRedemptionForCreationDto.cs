using System.ComponentModel.DataAnnotations;

namespace TapForPerksAPI.Models
{
    public class RewardRedemptionForCreationDto
    {

        [Required(ErrorMessage = "Reward ID is required")]
        public Guid RewardId { get; set; }

        [Required(ErrorMessage = "RewardOwnerUserId is required")]
        public Guid? RewardOwnerUserId { get; set; }

        [Required(ErrorMessage = "QR Code value is required")]
        public string QrCodeValue { get; set; } = null!;

        [Required(ErrorMessage = "NumRewardsToClaim is required")]
        [Range(1, 10, ErrorMessage = "NumRewardsToClaim must be at least 1")]
        public int NumRewardsToClaim { get; set; }
    }
}
