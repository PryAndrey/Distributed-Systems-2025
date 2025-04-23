using System.Text;
using System.Text.Json;

namespace RankCalculator;

public class CentrifugoModule
{
    private readonly HttpClient _httpClient;

    public CentrifugoModule()
    {
        try
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000") };
            _httpClient.DefaultRequestHeaders.Add("Authorization",
                "apikey buWxtDTK7-hcfk09ye1IyBXghr81AD5HxmfXfnaeBiQs1s8UXAludsajOPVCs__BxwJsCgxpVpakFdc9OqzDig");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CentrifugoModule: {ex.Message}");
            throw;
        }
    }

    public async Task PublishAsync(string channel, string data)
    {
        try
        {
            var request = new { method = "publish", @params = new { channel, data } };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api", content);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Centrifugo API returned error: {response.StatusCode}, Content: {responseContent}");
            }

            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Successfully published to channel {channel}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error publishing to Centrifugo channel on {channel}: {ex.Message}");
            throw;
        }
    }
}