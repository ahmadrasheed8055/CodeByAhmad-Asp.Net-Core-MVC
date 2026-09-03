using FitMind_API.Models;
using FitMind_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitMind_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly GeminiChatService _chatService;

        public ChatbotController(GeminiChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskChatbot([FromBody] ChatRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var response = await _chatService.GenerateChatResponseAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while communicating with the AI assistant.", details = ex.Message });
            }
        }
    }
}
