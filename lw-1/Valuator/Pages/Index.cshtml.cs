using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(IDBService idbService, IMessageQueueService messageQueueService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text)
    {
        var id = Guid.NewGuid().ToString();

        idbService.SaveText(id, text);

        var similarity = idbService.CalculateSimilarity(id, text);
        idbService.SaveSimilarity(id, similarity);
        
        await messageQueueService.SendEventAsync(id,"SimilarityCalculated", similarity);

        await messageQueueService.SendIdMessageAsync("text_queue", id);

        return RedirectToPage("/Summary", new { id });
    }
}