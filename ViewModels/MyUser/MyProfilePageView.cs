using System;
using System.Collections.Generic;
using System.Text;
using ViewModels.MyUser;

namespace ViewModels.MyUser
{
    public class MyProfilePageView
    {
        public MyUserProfileView Profile { get; set; } = new();

        public UserProfileEditView EditProfile { get; set; } = new();

        public ChangeEmailView ChangeEmail { get; set; } = new();

        public ChangePasswordView ChangePassword { get; set; } = new();

        public string CurrentEmail { get; set; } = string.Empty;
    }
}
