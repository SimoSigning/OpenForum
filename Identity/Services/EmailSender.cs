using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Identity.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            // Placeholder implementation.
            // Real email service can be added later.

            return Task.CompletedTask;
        }
    }
}
