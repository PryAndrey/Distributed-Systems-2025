﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private readonly IDatabase _redisDb;

    public SummaryModel(ILogger<SummaryModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        // TODO: (pa1) проинициализировать свойство Rank значением из БД (Redis)
        var rankValue = _redisDb.StringGet("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue, out double rank))
        {
            Rank = rank;
        }
        else
        {
            throw new ArgumentException(rankValue);
        }

        // TODO: (pa1) проинициализировать свойство Similarity значением из БД (Redis)
        var similarityValue = _redisDb.StringGet("SIMILARITY-" + id);
        if (similarityValue.HasValue && double.TryParse(similarityValue, out double similarity))
        {
            Similarity = similarity;
        }
        else
        {
            throw new ArgumentException(similarityValue);
        }
        Console.WriteLine($"Rank: {Rank}, Similarity: {Similarity}");
    }
}
