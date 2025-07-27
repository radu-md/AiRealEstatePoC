using AiRealEstate.Core.Services;
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

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return BadRequest("Prompt is required.");
        }

        var response = await _chatService.AskAsync(prompt);
        return Ok(new { response });
    }
}
