namespace EntityModels
{
    public class Topic
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CreatedByUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
