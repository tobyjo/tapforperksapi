namespace TapForPerksAPI.Models
{
    public class ScanEventDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LoyaltyProgrammeId { get; set; }
        public Guid? LoyaltyOwnerUserId { get; set; }
        public int PointsChange { get; set; }
        public DateTime ScannedAt { get; set; }
    }
}
