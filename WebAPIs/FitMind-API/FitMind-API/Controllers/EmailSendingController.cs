using FitMind_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSendingController : ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailSendingController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string receptor,  string subject, string body)
        {
           await emailService.sendEmail(receptor, subject, body);
            return Ok();
        }
    }
}
