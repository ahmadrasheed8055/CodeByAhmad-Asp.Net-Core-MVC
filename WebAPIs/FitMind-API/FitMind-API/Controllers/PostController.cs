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
using FitMind_API.Services;
using static System.Net.Mime.MediaTypeNames;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly FMDBContext _context;
        
        private readonly SightengineService _sightengineService;

        public PostController(FMDBContext context, DeepAiService deepAIService, SightengineService sightengineService)
        {
            _context = context;
      
            _sightengineService = sightengineService;
        }






        // GET: api/Post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddPost>>> GetAddPosts()
        {
            return await _context.AddPosts.ToListAsync();
        }

        // GET: api/Post/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AddPost>> GetAddPost(int id)
        {
            var addPost = await _context.AddPosts.FindAsync(id);

            if (addPost == null)
            {
                return NotFound();
            }

            return addPost;
        }

        // PUT: api/Post/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddPost(int id, AddPost addPost)
        {
            if (id != addPost.PostId)
            {
                return BadRequest();
            }

            _context.Entry(addPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddPostExists(id))
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

        // POST: api/Post
        [HttpPost("add-post")]
        public async Task<ActionResult> AddPost ([FromForm]  AddPostDTO addPostDto)
        {
            
            if (addPostDto == null)
            {
                return BadRequest("Post data is null");
            }

            var userDetail = await _context.AppUsers.FindAsync(addPostDto.UserId);
            if (userDetail == null)
            {
                return NotFound("User not found");
            }

            var categoryDetail = await _context.Categories.FindAsync(addPostDto.CategoryId);
            if (categoryDetail == null)
            {
                return NotFound("Category not found");
            }

            var addPost = new AddPost
            {  
                Title = addPostDto.Title,
                Description = addPostDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = addPostDto.IsPublished,
                ViewCount = 0,
                LikeCount = 0,
                IsDeleted = false,
                UserId = addPostDto.UserId,
                CategoryId = addPostDto.CategoryId
            };



            object sensitivityObj = null;
            // Handle image upload 
            if (addPostDto.PostImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
                var maxLength = 5 * 1024 * 1024;
                var fileExtension = Path.GetExtension(addPostDto.PostImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Extension error.");
                }

                //checking sensitivity
                sensitivityObj = await _sightengineService.CheckImageAsync(addPostDto.PostImage);

              
                
                if (addPostDto.PostImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("Image size must be less than 5 mbs");
                }


                using (var memoryStream = new MemoryStream())
                {
                    await addPostDto.PostImage.CopyToAsync(memoryStream);
                    addPost.PostImage = memoryStream.ToArray();
                }
            }

            _context.AddPosts.Add(addPost);
            await _context.SaveChangesAsync();

            return Ok(sensitivityObj);
        }



        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image file is required.");

            var sensitivityObj = await _sightengineService.CheckImageAsync(image);

            if (sensitivityObj == null)
            {
                throw new Exception("Unable to analyze image sensitivity.");
            }

            // Check if API status is not success
            if (sensitivityObj.Status != "success")
            {
                throw new Exception("Image analysis failed. Please try with a different image.");
            }

            // Check for high nudity content (raw or partial)
            if (sensitivityObj.Nudity != null)
            {
                if (sensitivityObj.Nudity.Raw > 0.5m)
                    throw new Exception("Image contains high raw nudity, not allowed.");

                if (sensitivityObj.Nudity.Partial > 0.5m)
                    throw new Exception("Image contains partial nudity, not allowed.");
            }

            // Check for offensive content
            if (sensitivityObj.Offensive != null)
            {
                if (sensitivityObj.Offensive.Prob > 0.5m)
                    throw new Exception("Image is considered offensive, not allowed.");
            }

            // Check for WAD (weapons, alcohol, drugs) content if available
            if (sensitivityObj.Wad != null)
            {
                if (sensitivityObj.Wad.Weapons > 0.5m)
                    throw new Exception("Image contains weapons, not allowed.");

                if (sensitivityObj.Wad.Alcohol > 0.5m)
                    throw new Exception("Image contains alcohol, not allowed.");

                if (sensitivityObj.Wad.Drugs > 0.5m)
                    throw new Exception("Image contains drugs, not allowed.");
            }

            return Ok(sensitivityObj); // JSON result
        }


        // DELETE: api/Post/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddPost(int id)
        {
            var addPost = await _context.AddPosts.FindAsync(id);
            if (addPost == null)
            {
                return NotFound();
            }

            _context.AddPosts.Remove(addPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AddPostExists(int id)
        {
            return _context.AddPosts.Any(e => e.PostId == id);
        }
    }
}
