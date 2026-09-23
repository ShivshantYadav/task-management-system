namespace TaskManagement.Application.Interfaces
{
    /// <summary>
    /// Sends transactional account emails (e.g. "your account was created, here are your
    /// sign-in credentials"). Implementations should never throw - a failed email must not
    /// block the underlying business operation (user/task creation, etc.). Instead they
    /// return false so the caller can decide how to surface that to the admin/manager
    /// (e.g. showing the temporary password on-screen so it can be shared manually).
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends the "your account was created" email containing the user's temporary
        /// password. Returns true if the email was handed off to the SMTP server
        /// successfully, false otherwise (including when email sending is disabled).
        /// </summary>
        Task<bool> SendAccountCreatedEmailAsync(
            string toEmail,
            string toName,
            string temporaryPassword,
            string role,
            string? teamName = null);
    }
}
