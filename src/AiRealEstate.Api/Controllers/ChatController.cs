using AiRealEstate.Api.Model;
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

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest prompt)
    {
        if (prompt is null || string.IsNullOrWhiteSpace(prompt.Message))
        {
            return BadRequest("Message is required.");
        }

        var reply = await _chatService.GetResponseAsync(prompt.Message);

        return Ok(new
        {
            response = reply,
            listings = new[] {
                new {
                    title = "Apartament modern în Cluj",
                    price = "550 EUR",
                    link = "https://romimo.ro/listing/123",
                    image = "https://romimo.ro/img.jpg"
                }
            },
            suggestedQuestions = new[] { "În ce cartier?", "Care este bugetul?" }
        });
    }
}
