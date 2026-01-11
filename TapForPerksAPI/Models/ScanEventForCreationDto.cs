namespace TapForPerksAPI.Models
{
    public class ScanEventForCreationDto
    {
        
        public Guid RewardId { get; set; }

        public Guid? LoyaltyOwnerUserId { get; set; }

        public string QrCodeValue { get; set; } = null!;

        public int PointsChange { get; set; }   // LO may want to reward more points than normal
    }
}
