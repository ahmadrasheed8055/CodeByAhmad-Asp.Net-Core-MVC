using FitMind_API.Data;
using FitMind_API.Models.DTOs;
using FitMind_API.Models.Entities;
using FitMind_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Text.RegularExpressions;
using static FitMind_API.Models.Enums.Enum;



namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSendingController : ControllerBase
    {
        private readonly IEmailService emailService;
        private readonly FMDBContext fMDBContext;

        public EmailSendingController(IEmailService emailService, FMDBContext fMDBContext)
        {
            this.emailService = emailService;
            this.fMDBContext = fMDBContext;
        }



        // Email validation function
        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(string receptor)
        {
            if (!IsValidEmail(receptor))
            {
                return BadRequest("Email format is not correct");
            }

            bool isUserActive = await fMDBContext.AppUsers.AnyAsync(u => u.Email == receptor);
            if (isUserActive)
            {
                return StatusCode(400);
            }
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(1); // adding 24 hours
            var registrationUrl = $"http://localhost:4200/register?token={token}";


            var registrationTokenDetail = new UserRT()
            {
                Email = receptor,
                Token = token,
                Status = 1,
                TokenType = 1,//for registration
                ExpiryDate = expiry, // expiring time
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            //next step is too
            this.fMDBContext.UserRegistrationTokens.Add(registrationTokenDetail);
            this.fMDBContext.SaveChanges();

            var subject = "Welcome to FitMind! Complete Your Registration 🚀";
            var body = $@"
                        <html>
                          <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; 
                                        border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                              <h1 style='color: #333333;'>Welcome to FitMind! 🎉</h1>
                              <p style='color: #555555; font-size: 16px;'>
                                Thank you for joining us. Click the button below to complete your registration and 
                                start your fitness journey!
                              </p>
                              <div style='margin: 30px 0;'>
                                <a href='{registrationUrl}' 
                                   style='background-color: #007bff; color: #ffffff; padding: 15px 25px; text-decoration: none; 
                                          border-radius: 5px; font-size: 16px; display: inline-block;'>
                                  Complete Registration
                                </a>
                              </div>
                              <p style='color: #777777; font-size: 14px;'>
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href='{registrationUrl}' style='color: #007bff; word-break: break-all;'>
                                {registrationUrl}
                                </a>
                              </p>
                              <p style='color: #777777; font-size: 14px;'>
                                See you soon!<br><b>The FitMind Team</b>
                              </p>
                            </div>
                          </body>
                        </html>";

            await emailService.sendEmail(receptor, subject, body);
            //return Redirect("www.google.com");
            return Ok(new { message = 1 });
        }



        //http://localhost:5177/api/EmailSending/validate-email-token?token=395ed8a9-8bed-409b-8e27-a3ba18ce2040
        //email validation
        [HttpGet("validate-email-token")]
        public async Task<IActionResult> emailValidation(string token)
        {
            //string urlToken = Request.Query["token"];
            string urlToken = token;

            if (urlToken == null)
            {
                return BadRequest(new { message = "Token is missing" });
            }

            var userToken = await fMDBContext.UserRegistrationTokens.FirstOrDefaultAsync(t => t.Token == urlToken);

            if (userToken == null)
            {
                return NotFound(new { message = "Token not found!" });
            }
            else if (userToken.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Token has expired!" });
            }
            else if (userToken.Status == 2)
            {
                //userToken.ExpiryDate = DateTime.UtcNow;
                return NotFound(new { message = "User is already active" });
            }



            return Ok(new { message = "Token found!", email = userToken.Email });

        }


        //=========Forget password functinality start from here===========
        [HttpPost("send-fp-email")]

        public async Task<IActionResult> SendFPEmail(string email)
        {
            //validate email
            if (!IsValidEmail(email))
            {
                return BadRequest("Email format is not correct");
            }

            //user existance check
            var user = await fMDBContext.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound("User not found");


            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(30); // adding 24 hours
            var fpPageUrl = $"http://localhost:4200/reset-password?token={token}";


            var resetToken = new UserRT()
            {
                Email = email,
                Token = token,
                Status = 1, // 1 for active/unused
                TokenType = 2, // 2 for ForgotPassword
                ExpiryDate = expiry, // expiring time
                InsertedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            this.fMDBContext.UserRegistrationTokens.Add(resetToken);
            this.fMDBContext.SaveChanges();

            var subject = "Reset Your FitMind Password 🔒";
            var body = $@"
                        <html>
                          <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; 
                                        border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); text-align: center;'>
                              <h1 style='color: #333333;'>Reset Your Password 🔑</h1>
                              <p style='color: #555555; font-size: 16px;'>
                                We received a request to reset your password. Click the button below to set a new password for your account.
                              </p>
                              <div style='margin: 30px 0;'>
                                <a href='{fpPageUrl}' 
                                   style='background-color: #007bff; color: #ffffff; padding: 15px 25px; text-decoration: none; 
                                          border-radius: 5px; font-size: 16px; display: inline-block;'>
                                  Reset Password
                                </a>
                              </div>
                              <p style='color: #777777; font-size: 14px;'>
                                If the button doesn't work, copy and paste this link into your browser:<br>
                                <a href='{fpPageUrl}' style='color: #007bff; word-break: break-all;'>
                                {fpPageUrl}
                                </a>
                              </p>
                              <p style='color: #777777; font-size: 14px;'>
                                Stay safe!<br><b>The FitMind Team</b>
                              </p>
                            </div>
                          </body>
                        </html>";

            await emailService.sendEmail(email, subject, body);
            //return Redirect("www.google.com");
            return Ok(new { message = 1 });
        }

        //http://localhost:5177/api/EmailSending/validate-reset-token?token=...
        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> validateResetToken(string token)
        {
            string urlToken = token;

            if (string.IsNullOrEmpty(urlToken))
            {
                return BadRequest(new { message = "Token is missing" });
            }

            // TokenType == 2 is for ForgotPassword
            var userToken = await fMDBContext.UserRegistrationTokens.FirstOrDefaultAsync(t => t.Token == urlToken && t.TokenType == 2);

            if (userToken == null)
            {
                return NotFound(new { message = "Token not found or invalid!" });
            }
            else if (userToken.ExpiryDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Token has expired!" });
            }
            else if (userToken.Status == 2)
            {
                return BadRequest(new { message = "Token has already been used" });
            }

            return Ok(new { message = "Token found!", email = userToken.Email });
        }

    }
}
