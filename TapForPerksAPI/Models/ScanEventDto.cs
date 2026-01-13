namespace TapForPerksAPI.Models
{
    public class ScanEventDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RewardId { get; set; }
        public Guid? RewardOwnerUserId { get; set; }

        public string QrCodeValue { get; set; }
        public int PointsChange { get; set; }
        public DateTime ScannedAt { get; set; }
    }
}
