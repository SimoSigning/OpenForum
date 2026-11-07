namespace ViewModels.MyActivity
{
    public class ActivityCommentEditView
    {
        public int Id { get; set; }

        public string? Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}