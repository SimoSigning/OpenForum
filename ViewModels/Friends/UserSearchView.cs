
namespace ViewModels.Friends
{
    public class UserSearchView
    {
        public string SearchQuery { get; set; } = string.Empty;

        public IEnumerable<UserSearchItemView> Users { get; set; }
            = Enumerable.Empty<UserSearchItemView>();

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; } = 1;
    }
}
