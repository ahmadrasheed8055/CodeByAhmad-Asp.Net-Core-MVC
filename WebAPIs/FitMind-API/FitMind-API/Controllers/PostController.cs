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
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly FMDBContext _context;

        private readonly SightengineService _sightengineService;

        public PostController(FMDBContext context, SightengineService sightengineService)
        {
            _context = context;
            _sightengineService = sightengineService;
        }

        //get posts
        [HttpGet("getUserPosts/{userId}")]
        public async Task<ActionResult<List<GetUserPostsDTO>>> GetUserPosts(int userId)
        {
            if (userId == 0)
            {
                return BadRequest(new { message = "User ID is empty." });
            }

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var Posts = await _context.AddPosts
                                    .Where(p => p.UserId == userId && p.IsPublished && !p.IsDeleted && p.PublishAt != null)
                                    .Include(p => p.Category)
                                    .Include(p => p.postReactions)
                                    .OrderByDescending(p => p.PublishAt)
                                    .Select(post => new GetUserPostsDTO
                                    {
                                        PostId = post.PostId,
                                        Title = post.Title,
                                        Description = post.Description,
                                        CreatedAt = post.CreatedAt,
                                        UpdatedAt = post.UpdatedAt,
                                        PublishAt = post.PublishAt,
                                        IsPublished = post.IsPublished,
                                        UserId = post.UserId,
                                        UserName = user.Username,
                                        CategoryId = post.CategoryId,
                                        CategoryName = post.Category.Name,
                                        PostImageUrl = post.PostImage != null ? Convert.ToBase64String(post.PostImage) : null,
                                        ViewCount = post.ViewCount,
                                        LikeCount = post.postReactions.Count(r => r.IsLike == true) == 0
                                                    ? (int?)null
                                                    : post.postReactions.Count(r => r.IsLike == true),

                                        DislikeCount = post.postReactions.Count(r => r.IsLike == false) == 0
                                                   ? (int?)null
                                                   : post.postReactions.Count(r => r.IsLike == false),
                                        IsReactedByMe = userId != 0
                                                                    ? post.postReactions
                                                                        .Where(r => r.UserId == userId)
                                                                        .Select(r => (bool?)r.IsLike)
                                                                        .FirstOrDefault() // returns null if not reacted
                                                                    : (bool?)null
                                    })
                                    .ToListAsync();




            if (!Posts.Any())
            {
                return NotFound(new { message = "No posts found for this user." });
            }

            return Ok(Posts);
        }

        //[Authorize]
        //get posts
        [HttpGet("getAllPosts")]
        public async Task<ActionResult<List<GetAllPostsDTO>>> GetAllPosts(int? userId)
        {
            var Posts = await _context.AddPosts
                                    .Where(p => p.IsPublished && !p.IsDeleted && p.PublishAt != null)
                                    .Include(p => p.User)
                                    .Include(p => p.Category)
                                    .Include(p => p.postReactions)
                                    .OrderByDescending(p => p.PublishAt)
                                    .Select(post => new GetAllPostsDTO
                                    {
                                        PostId = post.PostId,
                                        Title = post.Title,
                                        Description = post.Description,
                                        CreatedAt = post.CreatedAt,
                                        UpdatedAt = post.UpdatedAt,
                                        PublishAt = post.PublishAt,
                                        IsPublished = post.IsPublished,
                                        UserId = post.UserId,
                                        UserName = post.User.Username,
                                        UserImage = post.User.ProfilePhoto != null ? Convert.ToBase64String(post.User.ProfilePhoto) : null,
                                        CategoryId = post.CategoryId,
                                        CategoryName = post.Category.Name,
                                        PostImage = post.PostImage != null ? Convert.ToBase64String(post.PostImage) : null,
                                        ViewCount = post.ViewCount,
                                        LikeCount = post.postReactions.Count(r => r.IsLike == true) == 0
                                                    ? (int?)null
                                                    : post.postReactions.Count(r => r.IsLike == true),
                                        DislikeCount = post.postReactions.Count(r => r.IsLike == false) == 0
                                                   ? (int?)null
                                                   : post.postReactions.Count(r => r.IsLike == false),

                                        IsReactedByMe = userId.HasValue && userId != 0
                                                                    ? post.postReactions
                                                                        .Where(r => r.UserId == userId)
                                                                        .Select(r => (bool?)r.IsLike)
                                                                        .FirstOrDefault() // returns null if not reacted
                                                                    : (bool?)null
                                    })
                                    .ToListAsync();

            if (!Posts.Any())
            {
                return NotFound(new { message = "No posts found for this user." });
            }

            return Ok(Posts);
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

        [HttpGet("getPostImage/{userId}/{postId}")]
        public async Task<ActionResult<string>> GetPostImage(int userId, int postId)
        {
            var imageAddress = await _context.AddPosts.Where(u => u.UserId == userId && u.PostId == postId)
                .Select(p => p.PostImage)
                .FirstOrDefaultAsync();
            if (imageAddress == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            return Ok(imageAddress);
        }

        //get drafted post
        [HttpGet("getDrafts/{userId}")]
        public async Task<ActionResult<List<GetDraftedPostDTO>>> GetDraftedPosts(int userId)
        {
            if (userId == 0)
            {
                return BadRequest(new { message = "User ID is empty." });
            }

            var dPosts = await _context.AddPosts
                .Where(u => u.UserId == userId && u.IsPublished == false && u.IsDeleted == false)
                .Select(post => new GetDraftedPostDTO
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Description = post.Description,
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    IsPublished = post.IsPublished,
                    UserId = post.UserId,
                    CategoryId = post.CategoryId
                })
                .ToListAsync();

            if (!dPosts.Any())
            {
                return NotFound(new { message = "No drafts found for this user." });
            }

            return Ok(dPosts);
        }

        [HttpGet("IsDraftAvailable/{userId}")]
        public async Task<bool> IsDraftAvailable(int userId)
        {
            return await _context.AddPosts
                .AnyAsync(p => p.UserId == userId && p.IsPublished == false && p.IsDeleted == false);
        }




        // POST: api/Post
        [HttpPost("add-post")]
        public async Task<ActionResult> AddPost([FromForm] AddPostDTO addPostDto)
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
            var isValidDesc = await _sightengineService.CheckTextAsync(addPostDto.Description);

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

            //condition for draft
            DateTime? PublishAt = addPostDto.IsPublished ? DateTime.Now : null;
            var addPost = new AddPost
            {
                Title = addPostDto.Title,
                Description = addPostDto.Description,
                CreatedAt = DateTime.Now,
                PublishAt = PublishAt,
                UpdatedAt = null,
                IsPublished = addPostDto.IsPublished,
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

            return Ok(addPost.PostId);
        }


        //image analyzing
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

        //text analyzing
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

        //Update Post
        [HttpPut("updatePost/{userId}")]
        public async Task<IActionResult> UpdatePost(int userId, [FromForm] UpdatePostDTO updatePostDTO)
        {
            if (updatePostDTO == null)
            {
                return BadRequest("Post data is null");
            }

            var userDetail = await _context.AppUsers.FindAsync(userId);
            if (userDetail == null)
            {
                return NotFound("User not found");
            }

            var categoryDetail = await _context.Categories.FindAsync(updatePostDTO.CategoryId);
            if (categoryDetail == null)
            {
                return NotFound("Category not found");
            }
            //finding post
            var post = await _context.AddPosts.FindAsync(updatePostDTO.PostId);
            if (post == null)
            {
                return NotFound(new { message = "Post not found!" });
            }
            #region Title Moderation

            var isValidTitle = await _sightengineService.CheckTextAsync(updatePostDTO.Title);

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
            var isValidDesc = await _sightengineService.CheckTextAsync(updatePostDTO.Description);

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



            if (post != null)
            {
                post.Title = updatePostDTO.Title;
                post.Description = updatePostDTO.Description;
                post.UpdatedAt = DateTime.Now;
                //post.PublishAt = PD;
                post.IsPublished = updatePostDTO.IsPublished;
                post.CategoryId = updatePostDTO.CategoryId;

                // Handle PublishAt logic
                if (updatePostDTO.IsPublished)
                {
                    post.PublishAt = post.PublishAt ?? DateTime.Now; // Set now only if not already set
                }
                else
                {
                    post.PublishAt = null; // Unpublished posts have null PublishAt
                }

            }



            // Handle image upload 
            if (updatePostDTO.PostImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".png", ".jpeg" };
                var maxLength = 5 * 1024 * 1024;
                var fileExtension = Path.GetExtension(updatePostDTO.PostImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Extension error.");
                }


                //checking sensitivity
                var sensitivityObj = await _sightengineService.CheckImageAsync(updatePostDTO.PostImage);

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


                if (updatePostDTO.PostImage.Length > maxLength)
                {
                    return BadRequest("Image size must be less than 5 mbs");
                }


                using (var memoryStream = new MemoryStream())
                {
                    await updatePostDTO.PostImage.CopyToAsync(memoryStream);
                    post.PostImage = memoryStream.ToArray();
                }
            }

            _context.AddPosts.Update(post);
            await _context.SaveChangesAsync();
            return Ok(new { m = "User Updated successfully" });
        }

        //delete post image
        [HttpPut("deletePostPhoto/{userId}/{postId}")]
        public async Task<IActionResult> DeletePostPhoto(int userId, int postId)
        {
            var post = await _context.AddPosts
                .FirstOrDefaultAsync(p => p.UserId == userId && p.PostId == postId);

            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            if (post.PostImage == null || post.PostImage.Length == 0)
            {
                return BadRequest(new { message = "No image to delete" });
            }

            post.PostImage = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post image deleted successfully" });
        }



        //Soft-Delete Draft post
        [HttpPut("deleteDraftedPost/{userId}/{postId}")]
        public async Task<IActionResult> DeleteDraftedPost(int userId, int postId)
        {
            var addPost = await _context.AddPosts
                            .FirstOrDefaultAsync(p => p.UserId == userId && p.PostId == postId && p.IsPublished == false);

            if (addPost == null)
            {
                return NotFound(new { message = "Draft post not found." });
            }

            //soft delete
            addPost.IsDeleted = true;
            _context.AddPosts.Update(addPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Draft post deleted successfully." });
        }

        //Soft-Delete  post
        [HttpPut("deletePost/{userId}/{postId}")]
        public async Task<IActionResult> DeletePost(int userId, int postId)
        {
            var addPost = await _context.AddPosts
                            .FirstOrDefaultAsync(p => p.UserId == userId && p.PostId == postId && p.IsPublished == true);

            if (addPost == null)
            {
                return NotFound(new { message = "post not found." });
            }

            //soft delete
            addPost.IsDeleted = true;
            _context.AddPosts.Update(addPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "post deleted successfully." });
        }

        private bool AddPostExists(int id)
        {
            return _context.AddPosts.Any(e => e.PostId == id);
        }


        [HttpPost("add-comment")]
        public async Task<IActionResult> AddComment([FromBody] Models.DTOs.PostComments commentDto)
        {
            if (commentDto == null)
                return BadRequest("Comment data is null.");

            if (commentDto.PostId == 0 || commentDto.UserId == 0)
                return BadRequest(new { message = "PostId and UserId are required." });

            if (string.IsNullOrWhiteSpace(commentDto.CommentContent))
                return BadRequest(new { message = "CommentContent is required." });

            if (commentDto.CommentContent.Length > 1000)
                return BadRequest(new { message = "CommentContent exceeds maximum length of 1000 characters." });

            var user = await _context.AppUsers.FindAsync(commentDto.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var post = await _context.AddPosts.FindAsync(commentDto.PostId);
            if (post == null)
                return NotFound(new { message = "Post not found." });

            // Moderate comment text
            var moderation = await _sightengineService.CheckTextAsync(commentDto.CommentContent);
            if (moderation == null || moderation.Status != "success")
                return StatusCode(500, "Text moderation failed. Please try again.");

            var inappropriate = moderation.Profanity?.Matches?
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

            if (inappropriate != null && inappropriate.Any())
                return UnprocessableEntity(new { message = $"Comment contains inappropriate content." });

            var comment = new Models.Entities.PostComments
            {
                PostId = commentDto.PostId,
                UserId = commentDto.UserId,
                CommentContent = commentDto.CommentContent,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new { CommentId = comment.CommentId });
        }
    }
}
