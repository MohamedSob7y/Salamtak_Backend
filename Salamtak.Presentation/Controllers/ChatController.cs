using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Chat;

namespace Salamtak.presentation.Controllers // <-- Updated namespace!
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public ChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost("analyze-symptoms")]
        public async Task<IActionResult> AnalyzeSymptoms([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Please provide your symptoms.");
            }

            var response = await _aiChatService.GetMedicalSpecialtyAsync(request);

            return Ok(response);
        }
    }
}