using AiRealEstate.Api.Model;
using AiRealEstate.Core.Services;
using Azure;
using Microsoft.AspNetCore.Mvc;
using AiRealEstate.Api.Utilities;

namespace AiRealEstate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IWebHostEnvironment _env;

    public ChatController(IChatService chatService, IWebHostEnvironment env)
    {
        _chatService = chatService;
        _env = env;
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
            var json = ex.ToSafeJson(includeStack: _env.IsDevelopment(), maxDepth: 3);
            return new ContentResult { StatusCode = (int)ex.Status, Content = json, ContentType = "application/json" };
        }
        catch (Exception ex)
        {
            var json = ex.ToSafeJson(includeStack: _env.IsDevelopment(), maxDepth: 3);
            return new ContentResult { StatusCode = 500, Content = json, ContentType = "application/json" };
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
            var json = ex.ToSafeJson(includeStack: _env.IsDevelopment(), maxDepth: 2);
            return new ContentResult { StatusCode = 500, Content = json, ContentType = "application/json" };
        }
    }
}