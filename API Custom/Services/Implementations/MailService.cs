using API_Custom.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using System.Text;
using API_Custom.Models;
using API_Custom.ViewsModels;
using Microsoft.AspNetCore.Identity;

namespace API_Custom.Services.Implementations
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly IRazerViewRenderer _viewRenderer;
        private readonly IUtilsService _utilsService;
        private readonly UserManager<User> _userManager;
        private readonly DatabaseContext _databaseContext;

        private readonly string fromUsername;
        private readonly string fromEmail;


        public MailService(
            IConfiguration configuration,
            IRazerViewRenderer viewRenderer,
            IUtilsService utilsService,
            UserManager<User> userManager,
            DatabaseContext databaseContext
        )
        {
            _configuration = configuration;
            _viewRenderer = viewRenderer;
            _utilsService = utilsService;
            _databaseContext = databaseContext;
            _userManager = userManager;

            try
            {
                fromEmail = configuration.GetSection("SmtpClient").GetValue<string>("FromAddress") ?? "";
                fromUsername = configuration.GetSection("SmtpClient").GetValue<string>("FromName") ?? "";
            }
            catch (NullReferenceException)
            {
                Environment.Exit(1610); // ERROR_BAD_CONFIGURATION
            }
            
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using MailMessage mail = new();


            mail.From = new MailAddress(fromEmail, fromUsername);
            mail.To.Add(new MailAddress(to));

            mail.Subject = subject;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.Body = body;
            mail.BodyEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;

            try
            {
                using SmtpClient smtpClient = GenerateSMTPClient(_configuration);
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception)
            {
                throw new Exception("Error while sending email.");
            }
        }

        public async Task SendConfirmationRegisterEmailAsync(string emailTo)
        {
            var code = _utilsService.GenerateCode();

            var user = await _userManager.FindByEmailAsync(emailTo);

            if (user == null)
            {
                throw new Exception("User with provided email not found.");
            }

            user.EmailCode = code;

            await _databaseContext.SaveChangesAsync();

            var mailViewModel = new RegisterConfirmationViewModel
            {
                Code = code.ToString(),
            };

            string body = await _viewRenderer.RenderViewToStringAsync("./Views/Emails/Auth/Register/RegisterConfirmation.cshtml", mailViewModel);


            await SendEmailAsync(emailTo, "Confirmation", body);
        }

        private SmtpClient GenerateSMTPClient(IConfiguration configuration)
        {
            SmtpClient smtpClient = new();

            try
            {
                // SMTP CLIENT
                var hostSMTP = configuration.GetSection("SmtpClient").GetValue<string>("SMTPServerHost");
                var portSMTP = configuration.GetSection("SmtpClient").GetValue<string>("SMTPServerPort");
                var userSMTP = configuration.GetSection("SmtpClient").GetValue<string>("SMTPUsername");
                var passwordSMTP = configuration.GetSection("SmtpClient").GetValue<string>("SMTPPassword");
                var useSslSMTP = configuration.GetSection("SmtpClient").GetValue<bool>("SMTPUseSSL");

                if (String.IsNullOrWhiteSpace(portSMTP))
                {
                    smtpClient = new SmtpClient(hostSMTP);
                }
                else
                {
                    smtpClient = new SmtpClient(hostSMTP, Int32.Parse(portSMTP));
                }

                if (useSslSMTP)
                {
                    smtpClient.EnableSsl = true;
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((o, ce, ch, e) => true); // accept all certificates
                }
                else
                {
                    smtpClient.EnableSsl = false;
                }

                if (!String.IsNullOrWhiteSpace(userSMTP) && !String.IsNullOrWhiteSpace(passwordSMTP))
                {
                    smtpClient.Credentials = new NetworkCredential(userSMTP, passwordSMTP);
                }
                else
                {
                    smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

            }
            catch (Exception e) when (e is NullReferenceException || e is FormatException)
            {
                Environment.Exit(1610); // ERROR_BAD_CONFIGURATION
            }

            return smtpClient;
        }
    }
}
