namespace EventsLogger;

public class EventData
{
    public string EventType { get; set; } = string.Empty;
    public string TextId { get; set; } = string.Empty;
    public double? Rank { get; set; }
    public double? Similarity { get; set; }
}