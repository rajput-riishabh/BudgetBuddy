using System.Net;
using System.Net.Mail;

namespace BudgetBuddy.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load email settings from configuration
            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpServer = emailSettings["SmtpServer"];
            _smtpPort = int.Parse(emailSettings["SmtpPort"]);
            _senderEmail = emailSettings["SenderEmail"];
            _senderName = emailSettings["SenderName"];
            _smtpUsername = emailSettings["SmtpUsername"];
            _smtpPassword = emailSettings["SmtpPassword"];
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_senderEmail, _senderName);
                message.To.Add(new MailAddress(email));
                message.Subject = "Reset Your BudgetBuddy Password";
                message.Body = $@"
                    <html>
                    <body>
                        <h2>Reset Your Password</h2>
                        <p>Hello,</p>
                        <p>We received a request to reset your BudgetBuddy password. Click the link below to set a new password:</p>
                        <p><a href='{resetLink}'>Reset Password</a></p>
                        <p>If you didn't request this, you can safely ignore this email.</p>
                        <p>This link will expire in 24 hours.</p>
                        <br>
                        <p>Best regards,</p>
                        <p>The BudgetBuddy Team</p>
                    </body>
                    </html>";
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                await smtpClient.SendMailAsync(message);
                _logger.LogInformation($"Password reset email sent successfully to {email} at {DateTime.Parse("2025-03-11 15:03:29")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send password reset email to {email}: {ex.Message}");
                throw;
            }
        }
    }
}