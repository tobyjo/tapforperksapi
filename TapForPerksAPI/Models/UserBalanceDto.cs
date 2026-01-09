namespace TapForPerksAPI.Models
{
    public class UserBalanceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LoyaltyProgrammeId { get; set; }
        public int Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
