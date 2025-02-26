using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitMind_API.Data;
using FitMind_API.Models.Entities;
using FitMind_API.Models.DTOs;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUsersController : ControllerBase
    {
        private readonly FMDBContext _context;

        public AppUsersController(FMDBContext context)
        {
            _context = context;
        }

        // GET: api/AppUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUsers>>> GetAppUsers()
        {
            return await _context.AppUsers.ToListAsync();
        }

        // GET: api/AppUsers/5
        [HttpPost("login-user")]
        public async Task<ActionResult<AppUsers>> GetAppUsers([FromBody] UserLoginDTO loginDTO)
        {
            var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Email == loginDTO.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }
            else if (BCrypt.Net.BCrypt.Verify(loginDTO.HashedPassword, user.PasswordHash) == false)
            {
                return BadRequest("Wrong password");
            }

            return user;

        }







        // PUT: api/AppUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppUsers(int id, AppUsers appUsers)
        {
            if (id != appUsers.Id)
            {
                return BadRequest();
            }

            _context.Entry(appUsers).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppUsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AppUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("add-app-user")]
        public async Task<ActionResult> PostAppUsers(AppUserDTO uDTO)
        {
            // Step 1: Check if a valid token exists for the email
            var userToken = await _context.UserRegistrationTokens
                .FirstOrDefaultAsync(ut =>
                    ut.Email == uDTO.Email
                    && ut.Status == 1);

            if (userToken == null)
                return NotFound(new { message = "Token for this email not found!" });

            if (userToken.ExpiryDate < DateTime.UtcNow)
                return BadRequest(new { message = "Token has expired!" });

            // Step 2: Create user
            AppUsers user = new AppUsers()
            {
                Username = uDTO.Username,
                Email = uDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(uDTO.PasswordHash),
                EmailConfirmed = uDTO.EmailConfirmed,
                IsDeleted = false,
                JoinedDate = uDTO.JoinedDate,
                Status = 2
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync(); // Save user first

            // Step 3: Link token to user
            userToken.UserId = user.Id;
            userToken.Status = 2;
            _context.UserRegistrationTokens.Update(userToken);
            await _context.SaveChangesAsync(); // Save token update

            return Ok(new { message = "User registered successfully and token linked!" });
        }


        // DELETE: api/AppUsers/5
        [HttpPatch("delete-app-user/{id}")]
        public async Task<IActionResult> DeleteAppUsers(int id)
        {
            var appUsers = await _context.AppUsers.FindAsync(id);
            if (appUsers == null)
            {
                return NotFound(new { message = "User with this id does not exist!" });
            }
            appUsers.IsDeleted = true;
            _context.AppUsers.Update(appUsers);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User unactivated!" });
        }


        //uploading image in bytes 
        [HttpPut("upload-image/{id}")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file, int id)
        {
             //step 3 find user by id
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }
            //step 1
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file detected!");
            }

            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var maxSize = 5 * 1024 * 1024;
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (file.Length > maxSize)
            {
                return BadRequest("File size must be less than 5MB.");
            }
            else if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed.");
            }


            //step 2 file to byte array
            using var memory = new MemoryStream();
            await file.CopyToAsync(memory);

           

            user.ProfilePhoto = memory.ToArray();
            await _context.SaveChangesAsync();
            return Ok();

        }

        [HttpGet("get-image/{id}")]
        public async Task<IActionResult> GetProfileImage(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null || user.ProfilePhoto == null)
            {
                return NotFound();
            }

            return Ok( user.ProfilePhoto);
        }
        private bool AppUsersExists(int id)
        {
            return _context.AppUsers.Any(e => e.Id == id);
        }
    }
}
