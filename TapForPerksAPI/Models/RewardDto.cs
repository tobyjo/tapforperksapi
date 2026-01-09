namespace TapForPerksAPI.Models
{
    public class RewardDto
    {
        public Guid Id { get; set; }
        public Guid LoyaltyProgrammeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty;
        public int? CostPoints { get; set; }
        public int? MaxScans { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
