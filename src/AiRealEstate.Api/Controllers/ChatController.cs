using AiRealEstate.Api.Model;
using AiRealEstate.Core.Services;
using Azure;
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
        try
        {
            if (request is null)
            {
                return BadRequest("ChatRequest is required.");
            }

            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

            string aiModel = request.Model == "GPT 5 nano" ? "azure" : "vertex";

            var chatResponse = await _chatService.GetResponseAsync(aiModel, sessionId, request.Message);

            return Ok(chatResponse);
        }
        catch (RequestFailedException ex)
        {
            return StatusCode((int)ex.Status, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        try
        {
            return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}