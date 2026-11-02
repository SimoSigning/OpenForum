using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ViewModels.User
{
    public class ChangePasswordView
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
