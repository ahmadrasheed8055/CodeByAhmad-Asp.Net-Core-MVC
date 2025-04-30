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

            //Moderating post title and description           
            #region Title Moderation

            var isValidTitle = await _sightengineService.CheckTextAsync(addPostDto.Title);

            if (isValidTitle == null || isValidTitle.Status != "success")
                return StatusCode(500, "Text moderation failed. Please try again.");


            var inappropriateTitle = isValidTitle.Profanity?.Matches?
                .Where(m =>
                            m.Type == "inappropriate" ||
                            m.Type == "insult" ||
                            m.Type == "sexual" ||
                            m.Type == "hate" ||
                            m.Type == "threat" ||
                            m.Type == "violence" ||
                            m.Type == "profanity" ||
                            m.Type == "racist" ||
                            m.Type == "homophobic" ||
                            m.Type == "misogyny" ||
                            m.Type == "drugs" ||
                            m.Intensity == "high")
                .Select(m => m)
                .Distinct()
                .ToList();

            if (inappropriateTitle != null && inappropriateTitle.Any())
                return UnprocessableEntity($"Text contains inappropriate content: {string.Join(", ", inappropriateTitle)}");
            #endregion

            
            #region Description Moderation
            var isValidDesc = await _sightengineService.CheckTextAsync(addPostDto.Title);

            if (isValidDesc == null || isValidDesc.Status != "success")
                return StatusCode(500, "Text moderation failed. Please try again.");


            var inappropriateDesc = isValidDesc.Profanity?.Matches?
                .Where(m =>
                            m.Type == "inappropriate" ||
                            m.Type == "insult" ||
                            m.Type == "sexual" ||
                            m.Type == "hate" ||
                            m.Type == "threat" ||
                            m.Type == "violence" ||
                            m.Type == "profanity" ||
                            m.Type == "racist" ||
                            m.Type == "homophobic" ||
                            m.Type == "misogyny" ||
                            m.Type == "drugs" ||
                            m.Intensity == "high")
                .Select(m => m)
                .Distinct()
                .ToList();

            if (inappropriateDesc != null && inappropriateDesc.Any())
                return UnprocessableEntity($"Text contains inappropriate content: {string.Join(", ", inappropriateDesc)}");
            #endregion

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
                var sensitivityObj = await _sightengineService.CheckImageAsync(addPostDto.PostImage);
                
                //conditions
                if (sensitivityObj == null || sensitivityObj.Status != "success")
                    return StatusCode(500, "Image analysis failed. Please try again.");

                // Nudity check
                if (sensitivityObj.Nudity != null)
                {
                    if (sensitivityObj.Nudity.Raw > 0.5m)
                        return UnprocessableEntity("Image contains high raw nudity and is not allowed.");

                    if (sensitivityObj.Nudity.Partial > 0.5m)
                        return UnprocessableEntity("Image contains partial nudity and is not allowed.");
                }

                // Offensive content check
                if (sensitivityObj.Offensive != null && sensitivityObj.Offensive.Prob > 0.5m)
                    return UnprocessableEntity("Image is considered offensive and is not allowed.");

                // WAD (Weapons, Alcohol, Drugs) check
                if (sensitivityObj.Wad != null)
                {
                    if (sensitivityObj.Wad.Weapons > 0.5m)
                        return UnprocessableEntity("Image contains weapons and is not allowed.");

                    if (sensitivityObj.Wad.Alcohol > 0.5m)
                        return UnprocessableEntity("Image contains alcohol and is not allowed.");

                    if (sensitivityObj.Wad.Drugs > 0.5m)
                        return UnprocessableEntity("Image contains drugs and is not allowed.");
                }


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

            return Ok("User Added successfully");
        }



        [HttpPost("analyze-image")]
        public async Task<IActionResult> AnalyzeImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Image file is required.");

            var sensitivityObj = await _sightengineService.CheckImageAsync(image);

            if (sensitivityObj == null || sensitivityObj.Status != "success")
                return StatusCode(500, "Image analysis failed. Please try again.");

            // Nudity check
            if (sensitivityObj.Nudity != null)
            {
                if (sensitivityObj.Nudity.Raw > 0.5m)
                    return UnprocessableEntity("Image contains high raw nudity and is not allowed.");

                if (sensitivityObj.Nudity.Partial > 0.5m)
                    return UnprocessableEntity("Image contains partial nudity and is not allowed.");
            }

            // Offensive content check
            if (sensitivityObj.Offensive != null && sensitivityObj.Offensive.Prob > 0.5m)
                return UnprocessableEntity("Image is considered offensive and is not allowed.");

            // WAD (Weapons, Alcohol, Drugs) check
            if (sensitivityObj.Wad != null)
            {
                if (sensitivityObj.Wad.Weapons > 0.5m)
                    return UnprocessableEntity("Image contains weapons and is not allowed.");

                if (sensitivityObj.Wad.Alcohol > 0.5m)
                    return UnprocessableEntity("Image contains alcohol and is not allowed.");

                if (sensitivityObj.Wad.Drugs > 0.5m)
                    return UnprocessableEntity("Image contains drugs and is not allowed.");
            }

            return Ok(sensitivityObj); // Image passed all checks
        }

        [HttpPost("analyze-text")]
        public async Task<Boolean> AnalyzeText([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                //return BadRequest("Text is required.");
                return false;

            var result = await _sightengineService.CheckTextAsync(text);

            if (result == null || result.Status != "success")
                //return StatusCode(500, "Text moderation failed. Please try again.");
                return false;

            var inappropriateWords = result.Profanity?.Matches?
                .Where(m =>
                            m.Type == "inappropriate" ||
                            m.Type == "insult" ||
                            m.Type == "sexual" ||
                            m.Type == "hate" ||
                            m.Type == "threat" ||
                            m.Type == "violence" ||
                            m.Type == "profanity" ||
                            m.Type == "racist" ||
                            m.Type == "homophobic" ||
                            m.Type == "misogyny" ||
                            m.Type == "drugs" ||
                            m.Intensity == "high")
                .Select(m => m)
                .Distinct()
                .ToList();

            if (inappropriateWords != null && inappropriateWords.Any())
                //return UnprocessableEntity($"Text contains inappropriate content: {string.Join(", ", inappropriateWords)}");
                return false;

            //return Ok("Text is appropriate.");
            return true;
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
