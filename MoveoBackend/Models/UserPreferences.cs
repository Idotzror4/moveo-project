namespace MoveoBackend.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string InterestedAssets { get; set; } = string.Empty;
        public string InvestorType { get; set; } = string.Empty;
        public string ContentTypes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}