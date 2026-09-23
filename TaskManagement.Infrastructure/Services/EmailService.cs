using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAccountCreatedEmailAsync(
            string toEmail,
            string toName,
            string temporaryPassword,
            string role,
            string? teamName = null)
        {
            var section = _configuration.GetSection("Smtp");
            var enabled = section.GetValue<bool>("Enabled");

            var subject = "Your TaskFlow account has been created";
            var body = BuildBody(toName, toEmail, temporaryPassword, role, teamName);

            if (!enabled)
            {
                _logger.LogWarning(
                    "SMTP is not enabled (Smtp:Enabled=false). Skipping real email send. " +
                    "Would have emailed {ToEmail} with subject '{Subject}'. Temporary password: {Password}",
                    toEmail, subject, temporaryPassword);
                return false;
            }

            var host = section["Host"];
            var portRaw = section["Port"];
            var username = section["Username"];
            var password = section["Password"];
            var fromEmail = section["FromEmail"] ?? username;
            var fromName = section["FromName"] ?? "TaskFlow";
            var enableSsl = section.GetValue<bool?>("EnableSsl") ?? true;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogWarning(
                    "Smtp:Enabled is true but Smtp:Host/Smtp:FromEmail is missing. Skipping email to {ToEmail}.",
                    toEmail);
                return false;
            }

            var port = int.TryParse(portRaw, out var parsedPort) ? parsedPort : 587;

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl
                };

                if (!string.IsNullOrWhiteSpace(username))
                {
                    client.Credentials = new NetworkCredential(username, password);
                }

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(message);
                _logger.LogInformation("Account-created email sent to {ToEmail}.", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                // Never let a mail-server hiccup fail the user/team-creation request.
                _logger.LogError(ex, "Failed to send account-created email to {ToEmail}.", toEmail);
                return false;
            }
        }

        private static string BuildBody(string toName, string email, string temporaryPassword, string role, string? teamName)
        {
            var teamLine = string.IsNullOrWhiteSpace(teamName)
                ? string.Empty
                : $"<p>You have been added to the <strong>{teamName}</strong> team.</p>";

            return $@"
                <div style=""font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1f2937;"">
                    <h2 style=""margin-bottom:4px;"">Welcome to TaskFlow, {toName}!</h2>
                    <p>An account has been created for you with the <strong>{role}</strong> role.</p>
                    {teamLine}
                    <table style=""margin:16px 0;border-collapse:collapse;"">
                        <tr>
                            <td style=""padding:4px 12px 4px 0;color:#6b7280;"">Email</td>
                            <td style=""padding:4px 0;font-weight:600;"">{email}</td>
                        </tr>
                        <tr>
                            <td style=""padding:4px 12px 4px 0;color:#6b7280;"">Temporary password</td>
                            <td style=""padding:4px 0;font-weight:600;"">{temporaryPassword}</td>
                        </tr>
                    </table>
                    <p>Please sign in and change your password from your Profile page as soon as possible.</p>
                    <p style=""color:#6b7280;font-size:12px;"">If you weren't expecting this email, please contact your administrator.</p>
                </div>";
        }
    }
}
