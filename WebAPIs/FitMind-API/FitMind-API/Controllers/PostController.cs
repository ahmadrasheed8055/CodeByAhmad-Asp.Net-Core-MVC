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
using System.Security.Claims;
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
                                    .Where(p => p.UserId == userId && p.IsPublished && !p.IsDeleted && p.PublishAt != null && !_context.HiddenPosts.Any(hp => hp.PostId == p.PostId))
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
                                                                    : (bool?)null,
                                        IsSavedByMe = userId != 0
                                            ? _context.SavedPosts.Any(sp => sp.UserId == userId && sp.PostId == post.PostId)
                                            : (bool?)null,
                                        IsHidden = false
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimsUserId, out int parsedId))
                {
                    userId = parsedId;
                }
            }

            var Posts = await _context.AddPosts
                                    .Where(p => p.IsPublished && !p.IsDeleted && p.PublishAt != null && !_context.HiddenPosts.Any(hp => hp.PostId == p.PostId))
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
                                                                    : (bool?)null,
                                        IsSavedByMe = userId.HasValue && userId != 0
                                            ? _context.SavedPosts.Any(sp => sp.UserId == userId && sp.PostId == post.PostId)
                                            : (bool?)null,
                                        IsHidden = false
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

            if (IsTextInappropriate(isValidTitle))
                return UnprocessableEntity("Title contains inappropriate content.");
            #endregion

            #region Description Moderation
            var isValidDesc = await _sightengineService.CheckTextAsync(addPostDto.Description);

            if (IsTextInappropriate(isValidDesc))
                return UnprocessableEntity("Description contains inappropriate content.");
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

            if (IsTextInappropriate(result))
                return false;

            //return Ok("Text is appropriate.");
            return true;
        }

        //Update Post
        [HttpPut("updatePost/{userId}")]
        public async Task<IActionResult> UpdatePost(int userId, [FromForm] UpdatePostDTO updatePostDTO)
        {
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to edit this post." });
            }

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

            if (IsTextInappropriate(isValidTitle))
                return UnprocessableEntity("Title contains inappropriate content.");
            #endregion

            #region Description Moderation
            var isValidDesc = await _sightengineService.CheckTextAsync(updatePostDTO.Description);

            if (IsTextInappropriate(isValidDesc))
                return UnprocessableEntity("Description contains inappropriate content.");
            #endregion



            if (post != null)
            {
                bool wasPublishedBefore = post.IsPublished;

                post.Title = updatePostDTO.Title;
                post.Description = updatePostDTO.Description;
                post.IsPublished = updatePostDTO.IsPublished;
                post.CategoryId = updatePostDTO.CategoryId;

                // Handle PublishAt and UpdatedAt logic
                if (updatePostDTO.IsPublished)
                {
                    if (!wasPublishedBefore)
                    {
                        // Publishing for the first time from draft
                        post.PublishAt = DateTime.Now;
                        post.UpdatedAt = null;
                    }
                    else
                    {
                        // Updating an already published post
                        post.UpdatedAt = DateTime.Now;
                    }
                }
                else
                {
                    post.PublishAt = null;
                    post.UpdatedAt = DateTime.Now;
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
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to delete this photo." });
            }

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
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to delete this draft." });
            }

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
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to delete this post." });
            }

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
            if (IsTextInappropriate(moderation))
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
        [HttpPost("savePost/{userId}/{postId}")]
        public async Task<IActionResult> SavePost(int userId, int postId)
        {
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            var post = await _context.AddPosts.FindAsync(postId);
            if (post == null) return NotFound(new { message = "Post not found." });

            var existingSave = await _context.SavedPosts.FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);
            if (existingSave != null) return BadRequest(new { message = "Post is already saved." });

            var savedPost = new SavedPost { UserId = userId, PostId = postId, SavedAt = DateTime.UtcNow };
            _context.SavedPosts.Add(savedPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post saved successfully." });
        }

        [HttpDelete("unsavePost/{userId}/{postId}")]
        public async Task<IActionResult> UnsavePost(int userId, int postId)
        {
            var savedPost = await _context.SavedPosts.FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);
            if (savedPost == null) return NotFound(new { message = "Saved post not found." });

            _context.SavedPosts.Remove(savedPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post unsaved successfully." });
        }

        [HttpGet("getSavedPosts/{userId}")]
        public async Task<ActionResult<List<GetAllPostsDTO>>> GetSavedPosts(int userId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimsUserId, out int parsedId))
                {
                    userId = parsedId;
                }
            }

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            var Posts = await _context.SavedPosts
                                    .Where(sp => sp.UserId == userId && sp.Post.IsPublished && !sp.Post.IsDeleted && !_context.HiddenPosts.Any(hp => hp.PostId == sp.PostId))
                                    .Include(sp => sp.Post).ThenInclude(p => p.User)
                                    .Include(sp => sp.Post).ThenInclude(p => p.Category)
                                    .Include(sp => sp.Post).ThenInclude(p => p.postReactions)
                                    .OrderByDescending(sp => sp.SavedAt)
                                    .Select(sp => new GetAllPostsDTO
                                    {
                                        PostId = sp.Post.PostId,
                                        Title = sp.Post.Title,
                                        Description = sp.Post.Description,
                                        CreatedAt = sp.Post.CreatedAt,
                                        UpdatedAt = sp.Post.UpdatedAt,
                                        PublishAt = sp.Post.PublishAt,
                                        IsPublished = sp.Post.IsPublished,
                                        UserId = sp.Post.UserId,
                                        UserName = sp.Post.User.Username,
                                        UserImage = sp.Post.User.ProfilePhoto != null ? Convert.ToBase64String(sp.Post.User.ProfilePhoto) : null,
                                        CategoryId = sp.Post.CategoryId,
                                        CategoryName = sp.Post.Category.Name,
                                        PostImage = sp.Post.PostImage != null ? Convert.ToBase64String(sp.Post.PostImage) : null,
                                        ViewCount = sp.Post.ViewCount,
                                        LikeCount = sp.Post.postReactions.Count(r => r.IsLike == true) == 0 ? (int?)null : sp.Post.postReactions.Count(r => r.IsLike == true),
                                        DislikeCount = sp.Post.postReactions.Count(r => r.IsLike == false) == 0 ? (int?)null : sp.Post.postReactions.Count(r => r.IsLike == false),
                                        IsReactedByMe = sp.Post.postReactions.Where(r => r.UserId == userId).Select(r => (bool?)r.IsLike).FirstOrDefault(),
                                        IsSavedByMe = true,
                                        IsHidden = false
                                    })
                                    .ToListAsync();

            if (!Posts.Any()) return NotFound(new { message = "No saved posts found for this user." });
            return Ok(Posts);
        }

        [HttpPost("hidePost/{userId}/{postId}")]
        public async Task<IActionResult> HidePost(int userId, int postId)
        {
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to hide this post." });
            }

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            var post = await _context.AddPosts.FindAsync(postId);
            if (post == null) return NotFound(new { message = "Post not found." });
            if (post.UserId != userId) return Unauthorized(new { message = "You can only hide your own posts." });

            var existingHide = await _context.HiddenPosts.FirstOrDefaultAsync(hp => hp.UserId == userId && hp.PostId == postId);
            if (existingHide != null) return BadRequest(new { message = "Post is already hidden." });

            var hiddenPost = new HiddenPost { UserId = userId, PostId = postId, HiddenAt = DateTime.UtcNow };
            _context.HiddenPosts.Add(hiddenPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post hidden successfully." });
        }

        [HttpDelete("unhidePost/{userId}/{postId}")]
        public async Task<IActionResult> UnhidePost(int userId, int postId)
        {
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to unhide this post." });
            }

            var hiddenPost = await _context.HiddenPosts.FirstOrDefaultAsync(hp => hp.UserId == userId && hp.PostId == postId);
            if (hiddenPost == null) return NotFound(new { message = "Hidden post not found." });

            _context.HiddenPosts.Remove(hiddenPost);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post unhidden successfully." });
        }

        [HttpGet("getHiddenPosts/{userId}")]
        public async Task<ActionResult<List<GetAllPostsDTO>>> GetHiddenPosts(int userId)
        {
            var claimsUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claimsUserId == null || !int.TryParse(claimsUserId, out int parsedId) || parsedId != userId)
            {
                return Unauthorized(new { message = "You are not authorized to view these hidden posts." });
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = parsedId;
            }

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found." });

            var Posts = await _context.HiddenPosts
                                    .Where(hp => hp.UserId == userId && hp.Post.IsPublished && !hp.Post.IsDeleted)
                                    .Include(hp => hp.Post).ThenInclude(p => p.User)
                                    .Include(hp => hp.Post).ThenInclude(p => p.Category)
                                    .Include(hp => hp.Post).ThenInclude(p => p.postReactions)
                                    .OrderByDescending(hp => hp.HiddenAt)
                                    .Select(hp => new GetAllPostsDTO
                                    {
                                        PostId = hp.Post.PostId,
                                        Title = hp.Post.Title,
                                        Description = hp.Post.Description,
                                        CreatedAt = hp.Post.CreatedAt,
                                        UpdatedAt = hp.Post.UpdatedAt,
                                        PublishAt = hp.Post.PublishAt,
                                        IsPublished = hp.Post.IsPublished,
                                        UserId = hp.Post.UserId,
                                        UserName = hp.Post.User.Username,
                                        UserImage = hp.Post.User.ProfilePhoto != null ? Convert.ToBase64String(hp.Post.User.ProfilePhoto) : null,
                                        CategoryId = hp.Post.CategoryId,
                                        CategoryName = hp.Post.Category.Name,
                                        PostImage = hp.Post.PostImage != null ? Convert.ToBase64String(hp.Post.PostImage) : null,
                                        ViewCount = hp.Post.ViewCount,
                                        LikeCount = hp.Post.postReactions.Count(r => r.IsLike == true) == 0 ? (int?)null : hp.Post.postReactions.Count(r => r.IsLike == true),
                                        DislikeCount = hp.Post.postReactions.Count(r => r.IsLike == false) == 0 ? (int?)null : hp.Post.postReactions.Count(r => r.IsLike == false),
                                        IsReactedByMe = hp.Post.postReactions.Where(r => r.UserId == userId).Select(r => (bool?)r.IsLike).FirstOrDefault(),
                                        IsSavedByMe = _context.SavedPosts.Any(sp => sp.UserId == userId && sp.PostId == hp.PostId),
                                        IsHidden = true
                                    })
                                    .ToListAsync();

            if (!Posts.Any()) return NotFound(new { message = "No hidden posts found for this user." });
            return Ok(Posts);
        }

        private bool IsTextInappropriate(SightengineTextModerationDTO moderation)
        {
            if (moderation == null || moderation.Status != "success")
                return true;

            var ruleMatch = moderation.Profanity?.Matches?.Any(m =>
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
                    m.Intensity == "high") ?? false;

            if (ruleMatch) return true;

            if (moderation.ModerationClasses != null)
            {
                var ml = moderation.ModerationClasses;
                if (ml.Discriminatory > 0.5m || ml.Insulting > 0.5m || ml.Toxic > 0.5m || ml.Sexual > 0.5m)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
