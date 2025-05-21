using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class IndexModel(IDBService idbService, IMessageQueueService messageQueueService) : PageModel
{
    public void OnGet()
    {
    }
    
    public async Task<IActionResult> OnPost(string text, string country)
    {
        var id = Guid.NewGuid().ToString();

        var region = GetRegionByCountry(country);

        await idbService.SaveRegion(id, region);
        
        await idbService.SaveText(id, text);

        var similarity = idbService.CalculateSimilarity(id, text);
        await idbService.SaveSimilarity(id, similarity);

        await messageQueueService.SendEventAsync(id,"SimilarityCalculated", similarity);

        await messageQueueService.SendIdMessageAsync("text_queue", id);

        return RedirectToPage("/Summary", new { id });
    }

    private string GetRegionByCountry(string country)
    {
        return country switch
        {
            "Russia" => "RU",
            "France" => "EU",
            "Germany" => "EU",
            "UAE" => "ASIA",
            "India" => "ASIA",
            _ => throw new ArgumentException($"Unknown country: {country}")
        };
    }
    
}