namespace ViewModels.MyActivity
{
    public class MyActivityIndexView
    {
        public IEnumerable<ActivityListItemView> Activities { get; set; }
            = new List<ActivityListItemView>();

        public ActivityFilter Filter { get; set; }
            = ActivityFilter.All;

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }
    }
}