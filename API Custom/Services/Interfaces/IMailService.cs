namespace API_Custom.Services.Interfaces
{
    public interface IMailService
    {
        public Task SendEmailAsync(string to, string subject, string body);
        public Task SendConfirmationRegisterEmailAsync(string emailTo);
    }
}
