using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.User;

namespace ViewModels.User
{
    public class ProfilePageView
    {
        public UserProfileView Profile { get; set; } = new();

        public UserProfileEditView EditProfile { get; set; } = new();

        public ChangeEmailView ChangeEmail { get; set; } = new();

        public ChangePasswordView ChangePassword { get; set; } = new();

        public string CurrentEmail { get; set; } = string.Empty;
    }
}
