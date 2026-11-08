namespace EntityModels
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Address { get; set; }

        public string? PostalCode { get; set; }

        public string? City { get; set; }

        public string? ProfileText { get; set; }
    }
}