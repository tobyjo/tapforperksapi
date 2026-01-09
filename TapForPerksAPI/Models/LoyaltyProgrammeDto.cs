namespace TapForPerksAPI.Models
{
    public class LoyaltyProgrammeDto
    {
        public Guid Id { get; set; }
        public Guid LoyaltyOwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
