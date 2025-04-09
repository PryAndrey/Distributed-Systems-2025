using StackExchange.Redis;

namespace Valuator.Services;

public interface IRedisService
{
    void SaveText(string id, string text);
    void SaveSimilarity(string id, double similarity);
    double CalculateSimilarity(string id, string text);
}

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public void SaveText(string id, string text)
    {
        _redisDb.StringSet("TEXT-" + id, text);
    }

    public void SaveSimilarity(string id, double similarity)
    {
        _redisDb.StringSet("SIMILARITY-" + id, similarity);
    }

    public double CalculateSimilarity(string id, string text)
    {
        var keys = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First()).Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var storedText = _redisDb.StringGet(key);
            if (storedText == text && key != "TEXT-" + id)
            {
                return 1;
            }
        }

        return 0;
    }
}