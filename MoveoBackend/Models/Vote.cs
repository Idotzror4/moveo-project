namespace MoveoBackend.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string SectionType { get; set; } = string.Empty;
        public bool IsPositive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}