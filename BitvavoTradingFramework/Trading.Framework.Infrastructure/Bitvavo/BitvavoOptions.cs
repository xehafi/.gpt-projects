namespace Trading.Framework.Infrastructure.Bitvavo;

public sealed class BitvavoOptions
{
    public string WsUrl { get; set; } = "wss://ws.bitvavo.com/v2/";
    public string[] Markets { get; set; } = Array.Empty<string>();
    public int PingIntervalSeconds { get; set; } = 25;
    public ReconnectOptions Reconnect { get; set; } = new();
    public sealed class ReconnectOptions
    {
        public int InitialDelayMs { get; set; } = 500;
        public int MaxDelayMs { get; set; } = 30_000;
        public int JitterMs { get; set; } = 500;
    }
}
