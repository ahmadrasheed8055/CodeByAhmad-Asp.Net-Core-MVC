using System.Net;
using System.Net.Mail;

namespace FitMind_API.Services
{
    public interface IEmailService
    {
        Task sendEmail(string receptor, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task sendEmail(string receptor, string subject, string body)
        {
            var email = configuration.GetValue<string>("EMAIL_CONFIGRATION:EMAIL");
            var password = configuration.GetValue<string>("EMAIL_CONFIGRATION:PASSWORD");
            var host = configuration.GetValue<string>("EMAIL_CONFIGRATION:HOST");
            var port = configuration.GetValue<int>("EMAIL_CONFIGRATION:PORT");

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential(email, password);

            var message = new MailMessage(email!, receptor, subject, body);
            await smtpClient.SendMailAsync(message);

        }
    }
}
