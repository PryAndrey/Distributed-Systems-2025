using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redisDb;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey
        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        _redisDb.StringSet(rankKey, rank.ToString());

        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text);
        _redisDb.StringSet(similarityKey, similarity);
        
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        string textKey = "TEXT-" + id;
        _redisDb.StringSet(textKey, text);
        
        Console.WriteLine($"Text: {text}, Rank: {rank}, Similarity: {similarity}");
        return Redirect($"summary?id={id}");
    }
    
    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }
        
        int isNotLetterCount = text.Count(word => !char.IsLetter(word));

        return Math.Round((double)isNotLetterCount / text.Length, 2);
    }

    private double CalculateSimilarity(string text)
    {
        var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: "TEXT-*");
        foreach (var key in keys)
        {
            var value = _redisDb.StringGet(key);
            if (value == text)
            {
                return 1;
            }
        }

        return 0;
    }
}
