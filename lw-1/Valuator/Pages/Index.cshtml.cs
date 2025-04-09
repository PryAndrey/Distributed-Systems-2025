using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(IRedisService redisService, IMessageQueueService messageQueueService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text)
    {
        var id = Guid.NewGuid().ToString();

        redisService.SaveText(id, text);

        var similarity = redisService.CalculateSimilarity(id, text);
        redisService.SaveSimilarity(id, similarity);

        await messageQueueService.PublishMessageAsync("text_queue", id);

        return RedirectToPage("/Summary", new { id });
    }
}