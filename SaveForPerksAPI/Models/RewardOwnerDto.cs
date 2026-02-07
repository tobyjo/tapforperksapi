namespace SaveForPerksAPI.Models
{
    public class RewardOwnerDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }

        public Guid? CategoryId { get; set; }
        public RewardOwnerCategoryDto? Category { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
