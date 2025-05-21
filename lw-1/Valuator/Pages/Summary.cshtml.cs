using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;
using Valuator.Services;

namespace Valuator.Pages;

public class SummaryModel : PageModel
{
    private readonly IDBService _dbService;
    private readonly ILogger<SummaryModel> _logger;

    public double? Rank { get; private set; }
    public double Similarity { get; private set; }

    public SummaryModel(IDBService dbService, ILogger<SummaryModel> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }
    
    public async Task OnGet(string id)
    {
        await TryGetData(id);
    }
    
    public async Task<JsonResult> OnGetCheckData(string id)
    {
        await TryGetData(id);

        return new JsonResult(new
        {
            rank = Rank,
            similarity = Similarity
        });
    }
    
    private async Task TryGetData(string id)
    {
        var region = await _dbService.GetRegionForId(id);
        var regionalDb = _dbService.GetRegionalDb(region);

        var rankValue = await regionalDb.StringGetAsync("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue.ToString(), out var rank)) Rank = rank;

        var similarityValue = await regionalDb.StringGetAsync("SIMILARITY-" + id);
        if (similarityValue.HasValue && double.TryParse(similarityValue.ToString(), out var similarity))
            Similarity = similarity;
        _logger.LogInformation($"LOOKUP: {id}, {region}");
    }
}