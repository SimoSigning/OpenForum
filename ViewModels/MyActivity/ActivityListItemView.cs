namespace ViewModels.MyActivity
{
    public class ActivityListItemView
    {
        public int Id { get; set; }

        public ActivityType Type { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ContentPreview { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public int? TopicId { get; set; }

        public int? PostId { get; set; }

        public int? ParentCommentId { get; set; }

        public string ContextTitle { get; set; } = string.Empty;
    }
}