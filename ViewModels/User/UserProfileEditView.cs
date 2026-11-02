using System.ComponentModel.DataAnnotations;

namespace ViewModels.User
{
    public class UserProfileEditView
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }
    }
}