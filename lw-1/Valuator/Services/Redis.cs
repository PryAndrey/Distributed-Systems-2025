using System.Security.Cryptography;
using System.Text;
using StackExchange.Redis;

namespace Valuator.Services;

public interface IDBService
{
    Task SaveRegion(string id, string region);
    Task SaveText(string id, string text, string userLogin);
    Task SaveSimilarity(string id, double similarity);
    double CalculateSimilarity(string id, string text);
    Task<string> GetRegionForId(string id);
    IDatabase GetRegionalDb(string region);
    
    Task<bool> RegisterUser(string login, string password);
    Task<bool> ValidateUser(string login, string password);
    Task<string?> GetAuthorOfText(string id);
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

        var mainConn = configuration["DB_MAIN"] ?? Environment.GetEnvironmentVariable("DB_MAIN");
        var mainRedis = ConnectionMultiplexer.Connect(mainConn!);
        _redisDb = mainRedis.GetDatabase();

        _regionalDbDictionary = new Dictionary<string, IDatabase>
        {
            ["RU"] = ConnectionMultiplexer.Connect(configuration["DB_RU"] ?? Environment.GetEnvironmentVariable("DB_RU")!).GetDatabase(),
            ["EU"] = ConnectionMultiplexer.Connect(configuration["DB_EU"] ?? Environment.GetEnvironmentVariable("DB_EU")!).GetDatabase(),
            ["ASIA"] = ConnectionMultiplexer.Connect(configuration["DB_ASIA"] ?? Environment.GetEnvironmentVariable("DB_ASIA")!).GetDatabase()
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

    public async Task SaveText(string id, string text, string userLogin)
    {
        var regionalDb = GetRegionalDb(_currentRegionalName);
        await regionalDb.StringSetAsync($"TEXT-{id}", text);
        await _redisDb.StringSetAsync($"AUTHOR-{id}", userLogin);
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
    
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    public async Task<bool> RegisterUser(string login, string password)
    {
        var hash = HashPassword(password);
        var exists = await _redisDb.StringGetAsync($"USER-{login}");
        if (exists.HasValue) return false;
        await _redisDb.StringSetAsync($"USER-{login}", hash);
        return true;
    }

    public async Task<bool> ValidateUser(string login, string password)
    {
        var hash = HashPassword(password);
        var stored = await _redisDb.StringGetAsync($"USER-{login}");
        return stored.HasValue && stored.ToString() == hash;
    }

    public async Task<string?> GetAuthorOfText(string id)
    {
        var author = await _redisDb.StringGetAsync($"AUTHOR-{id}");
        if (!author.HasValue) return null;
        return author.ToString();
    }
}