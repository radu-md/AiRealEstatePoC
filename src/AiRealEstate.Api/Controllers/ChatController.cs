using AiRealEstate.Api.Model;
using AiRealEstate.Core.Services;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace AiRealEstate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest request)
    {
        if (request is null)
        {
            return BadRequest("ChatRequest is required.");
        }

        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault()
                    ?? Guid.NewGuid().ToString();

        var chatResponse = await _chatService.GetResponseAsync(sessionId, request.Message);

        return Ok(chatResponse);
    }
}
