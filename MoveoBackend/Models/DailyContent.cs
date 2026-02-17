namespace MoveoBackend.Models
{
    public class DailyContent
    {
        public int Id { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ContentData { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
