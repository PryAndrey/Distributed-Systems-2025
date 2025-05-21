using StackExchange.Redis;

namespace Valuator.Services;

public interface IDBService
{
    Task SaveRegion(string id, string region);
    Task SaveText(string id, string text);
    Task SaveSimilarity(string id, double similarity);
    double CalculateSimilarity(string id, string text);
    Task<string> GetRegionForId(string id);
    IDatabase GetRegionalDb(string region);
}

public class Redis : IDBService
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<Redis> _logger;
    private readonly Dictionary<string, IDatabase> _regionalDbDictionary;
    private string _currentRegionalName;

    public Redis(IConfiguration configuration, ILogger<Redis> logger)
    {
        _logger = logger;

        var mainRedis = ConnectionMultiplexer.Connect(configuration["DB_MAIN"]!);
        _redisDb = mainRedis.GetDatabase();

        _regionalDbDictionary = new Dictionary<string, IDatabase>
        {
            ["RU"] = ConnectionMultiplexer.Connect(configuration["DB_RU"]!).GetDatabase(),
            ["EU"] = ConnectionMultiplexer.Connect(configuration["DB_EU"]!).GetDatabase(),
            ["ASIA"] = ConnectionMultiplexer.Connect(configuration["DB_ASIA"]!).GetDatabase()
        };
    }

    public IDatabase GetRegionalDb(string region)
    {
        if (!_regionalDbDictionary.TryGetValue(region, out var db))
            throw new ArgumentException($"Redis database for region '{region}' not configured.");

        return db;
    }

    public async Task SaveRegion(string id, string region)
    {
        await _redisDb.StringSetAsync($"REGION-{id}", region);
        _currentRegionalName = region;

        _logger.LogInformation($"LOOKUP: {id}, MAIN");
    }

    public async Task SaveText(string id, string text)
    {
        var regionalDb = GetRegionalDb(_currentRegionalName);
        await regionalDb.StringSetAsync($"TEXT-{id}", text);

        _logger.LogInformation($"LOOKUP: {id}, {_currentRegionalName}");
    }

    public async Task SaveSimilarity(string id, double similarity)
    {
        var regionalDb = GetRegionalDb(_currentRegionalName);
        await regionalDb.StringSetAsync($"SIMILARITY-{id}", similarity);

        _logger.LogInformation($"LOOKUP: {id}, {_currentRegionalName}");
    }

    public double CalculateSimilarity(string id, string text)
    {
        var regionalDb = GetRegionalDb(_currentRegionalName);

        var keys = regionalDb.Multiplexer.GetServer(regionalDb.Multiplexer.GetEndPoints().First())
            .Keys(pattern: "TEXT-*");

        _logger.LogInformation($"LOOKUP: {id}, {_currentRegionalName}");

        foreach (var key in keys)
        {
            var storedText = regionalDb.StringGet(key);
            if (storedText == text && key != "TEXT-" + id) return 1;
        }

        return 0;
    }

    public async Task<string> GetRegionForId(string id)
    {
        var regionValue = await _redisDb.StringGetAsync($"REGION-{id}");
        if (!regionValue.HasValue) throw new KeyNotFoundException($"Region not found for ID: {id}");

        _logger.LogInformation($"LOOKUP: {id}, MAIN");

        return regionValue.ToString();
    }
}