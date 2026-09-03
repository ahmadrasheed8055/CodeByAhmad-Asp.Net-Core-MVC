using FitMind_API.Data;
using FitMind_API.Models;
using FitMind_API.Models.DTOs;
using FitMind_API.Models.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FitMind_API.Controllers
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly FMDBContext _context;
        private readonly AdminSettings _adminSettings;
        private readonly IConfiguration _config;

        public AdminAuthController(FMDBContext context, IOptions<AdminSettings> adminSettings, IConfiguration config)
        {
            _context = context;
            _adminSettings = adminSettings.Value;
            _config = config;
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] AdminGoogleLoginRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
                return BadRequest("Invalid ID token.");

            try
            {
                // Validate Google Token
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _config["GoogleOAuth:ClientId"] }
                };
                
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                
                if (payload == null)
                    return Unauthorized("Invalid Google authentication.");

                var email = payload.Email.ToLower().Trim();

                // Check Admin whitelist/DB
                var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email.ToLower() == email);
                
                // If it's the very first admin and they match the WhitelistedAdminEmails setting, bootstrap them
                if (admin == null && _adminSettings.WhitelistedAdminEmails.Any(e => e.ToLower().Trim() == email))
                {
                    admin = new AdminUser
                    {
                        Email = email,
                        DisplayName = payload.Name,
                        FirstLoginAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.AdminUsers.Add(admin);
                    await _context.SaveChangesAsync();
                }
                else if (admin == null)
                {
                    return StatusCode(403, "This Google account is not authorized for admin access.");
                }
                else if (!admin.IsActive)
                {
                    return StatusCode(403, "This admin account is deactivated.");
                }
                else
                {
                    // Update LastLoginAt
                    admin.LastLoginAt = DateTime.UtcNow;
                    if (string.IsNullOrEmpty(admin.DisplayName))
                    {
                        admin.DisplayName = payload.Name;
                    }
                    await _context.SaveChangesAsync();
                }

                // Generate Admin JWT
                var token = GenerateAdminJwt(admin);

                return Ok(new
                {
                    token,
                    adminId = admin.AdminId,
                    email = admin.Email,
                    displayName = admin.DisplayName
                });
            }
            catch (InvalidJwtException)
            {
                return Unauthorized("Invalid Google ID token signature.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateAdminJwt(AdminUser admin)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.AdminId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, admin.Email),
                new Claim("role", "admin"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
