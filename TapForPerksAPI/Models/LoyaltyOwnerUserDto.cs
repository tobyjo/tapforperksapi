namespace TapForPerksAPI.Models
{
    public class LoyaltyOwnerUserDto
    {
        public Guid Id { get; set; }
        public Guid LoyaltyOwnerId { get; set; }
        public string AuthProviderId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
