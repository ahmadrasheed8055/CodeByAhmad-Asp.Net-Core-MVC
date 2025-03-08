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
using System.Text.Json;
using FitMind_API.Common;

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
        public async Task<ActionResult<int>> GetAppUsers([FromBody] UserLoginDTO loginDTO)
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

            return user.Id;

        }

    

        [HttpGet("get-user/{id}")]
        public async Task<ActionResult<PublicAppUserDTO>> GetUser(int id)
        {
            var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Id == id);
                

            var result = JsonSerializer.Deserialize<PublicAppUserDTO>(JsonSerializer.Serialize(user));
          
            if (user == null || result == null)
            {
                return NotFound("User not found");
            }
            return result;
        }







        // PUT: api/AppUsers/
        [HttpPut("update-app-user/{id}")]
        public async Task<IActionResult> PutAppUsers(int id, PublicAppUserDTO appUser)
        {
            if (id != appUser.Id)
            {
                return BadRequest(new { message = "Invalid Id" });
            }

            var user = await _context.AppUsers.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            //  UniqueName UserVisibility Bio Phone FacebookLink InstagramLink  Location Country
            user.Username = appUser.Username;
            user.UniqueName = appUser.UniqueName;
            user.UserVisibility = appUser.UserVisibility;
            user.Bio = appUser.Bio;
            user.Phone = appUser.Phone;
            user.FacebookLink = appUser.FacebookLink;
            user.InstagramLink = appUser.InstagramLink;
            user.Location = appUser.Location;
            user.Country = appUser.Country;
            user.UpdatedAt = DateTime.UtcNow;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new {message = "User updated!"});
        }

        
        [HttpPost("add-app-user")]
        public async Task<ActionResult> PostAppUsers(RegistrationAppUserDTO uDTO)
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
                EmailConfirmed = true,
                IsDeleted = false,
                JoinedDate = DateTime.UtcNow,
                Status = 2
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync(); // Save user first

            // Step 3: Link token to user
            userToken.UserId = user.Id;
            userToken.Status = 2;
            _context.UserRegistrationTokens.Update(userToken);
            await _context.SaveChangesAsync(); // Save token update

            return Ok(new { message = "User registered successfully and token linked!", id = user.Id });
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
                return NotFound(new {message = "User not found"});
            }
            //step 1
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file detected" });
            }

            var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
            var maxSize = 5 * 1024 * 1024;
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (file.Length > maxSize)
            {
                return BadRequest(new { message = "File size must be less than 5MB." });
            }
            else if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Only JPG, JPEG, and PNG files are allowed." });
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


        //uploading background photo
        [HttpPut("upload-background-image/{id}")]
        public async Task<IActionResult> UploadBackgroundImage(IFormFile file, int id)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No image found." });
            }

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var allowedExtensions = new HashSet<string> { ".jpg", ".png", ".jpeg" };
            var maxSize = 5 * 1024 * 1024;
            var fileExtension = Path.GetExtension(file.FileName)?.ToLower();

            if (file.Length > maxSize || !allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new
                {
                    message = file.Length > maxSize
                    ? "File size exceeds 5MB."
                    : "Invalid file type. Only JPG, PNG, and JPEG are allowed."
                });
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            user.BackgroundPhoto = memoryStream.ToArray();
            await _context.SaveChangesAsync();

            return Ok(new { message = "Background image uploaded successfully." });
        }

        //get background image
        [HttpGet("get-background-image/{id}")]
        public async Task<IActionResult> GetBackgroundImage(int id)
        {
            if (id == 0)
            {
                return BadRequest(new { message = "No id detected" });
            }
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(user.BackgroundPhoto);
        }

        private bool AppUsersExists(int id)
        {
            return _context.AppUsers.Any(e => e.Id == id);
        }
    }
}
