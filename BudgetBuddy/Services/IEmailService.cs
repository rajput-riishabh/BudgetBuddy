namespace BudgetBuddy.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}