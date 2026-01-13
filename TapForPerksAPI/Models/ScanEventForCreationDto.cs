namespace TapForPerksAPI.Models
{
    public class ScanEventForCreationDto
    {
        
        public Guid RewardId { get; set; }

        public Guid? RewardOwnerUserId { get; set; }

        public string QrCodeValue { get; set; } = null!;

        public int PointsChange { get; set; }   // Bought more than 1 coffee
    }
}
