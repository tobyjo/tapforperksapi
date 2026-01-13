namespace TapForPerksAPI.Models
{
    public class RewardRedemptionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RewardId { get; set; }
        public Guid? RewardOwnerUserId { get; set; }
        public DateTime RedeemedAt { get; set; }
    }
}
