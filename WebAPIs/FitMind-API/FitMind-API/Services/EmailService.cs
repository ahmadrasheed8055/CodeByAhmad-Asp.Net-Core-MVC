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

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(email!),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(receptor);

            await smtpClient.SendMailAsync(message);
        }

    }
}
