using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using System;
using System.Threading.Tasks;

namespace Salamtak.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("{patientId}")]
        public async Task<IActionResult> Send(Guid patientId, [FromBody] string message)
        {
            var reply = await _chatService.SendMessageAsync(patientId, message);
            return Ok(reply);
        }

        [HttpGet("{patientId}/history")]
        public async Task<IActionResult> GetHistory(Guid patientId)
        {
            var history = await _chatService.GetHistoryAsync(patientId);
            return Ok(history);
        }
    }
}