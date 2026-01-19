namespace SaveForPerksAPI.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string AuthProviderId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string QrCodeValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
