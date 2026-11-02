using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.User
{
    public class ChangeEmailView
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string NewEmail { get; set; } = string.Empty;
    }
}
