using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public double Rank { get; private set; }
    public double Similarity { get; private set; }

    public void OnGet(string id)
    {
        TryGetData(id);
    }

    public JsonResult OnGetCheckData(string id)
    {
        bool isDataAvailable = TryGetData(id);
        
        return new JsonResult(new
        {
            isAvailable = isDataAvailable,
            rank = Rank,
            similarity = Similarity
        });
    }

    private bool TryGetData(string id)
    {
        bool hasRank = false;
        bool hasSimilarity = false;

        var rankValue = _redisDb.StringGet("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var rank))
        {
            Rank = rank;
            hasRank = true;
        }

        var similarityValue = _redisDb.StringGet("SIMILARITY-" + id);;
        
        if (similarityValue.HasValue && Double.TryParse(similarityValue, out var similarity))
        {
            Similarity = similarity;
            hasSimilarity = true;
        }

        Console.WriteLine($"Rank: {Rank}, Similarity: {Similarity}");
        return hasRank && hasSimilarity;
    }
}