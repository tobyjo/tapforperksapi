namespace TapForPerksAPI.Entities
{
    public class ScanEventForCreationDto
    {
        
        public Guid UserId { get; set; }

        public Guid LoyaltyProgrammeId { get; set; }

        public Guid? LoyaltyOwnerUserId { get; set; }

        public int PointsChange { get; set; }   // LO may want to reward more points than normal
    }
}
