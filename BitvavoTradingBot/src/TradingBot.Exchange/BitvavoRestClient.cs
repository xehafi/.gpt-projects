using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using TradingBot.Core.Exchange;
using TradingBot.Core.Models;
using static System.Net.WebRequestMethods;

namespace TradingBot.Exchange;

public sealed class BitvavoRestClient : IExchange
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _apiSecret;

    public BitvavoRestClient(string apiKey, string apiSecret, HttpClient? http = null)
    {
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _http = http ?? new HttpClient { BaseAddress = new Uri("https://api.bitvavo.com/v2/") };
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string? body = null, bool auth = false, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(method, path);
        if (body != null)
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        if (auth)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var prehash = ts + method.Method + "/" + path.TrimStart('/');
            if (body != null) prehash += body;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
            var sig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(prehash))).ToLowerInvariant();

            req.Headers.Add("Bitvavo-Access-Key", _apiKey);
            req.Headers.Add("Bitvavo-Access-Signature", sig);
            req.Headers.Add("Bitvavo-Access-Timestamp", ts);
            req.Headers.Add("Bitvavo-Access-Window", "10000");
        }

        return await _http.SendAsync(req, ct);
    }

    // Implement IExchange basics
    public async Task<DateTimeOffset> GetServerTime(CancellationToken ct = default)
    {
        var resp = await _http.GetFromJsonAsync<Dictionary<string, long>>("time", ct);
        return DateTimeOffset.FromUnixTimeMilliseconds(resp!["time"]);
    }

public async IAsyncEnumerable<Candle> GetCandles(
    string symbol,
    TimeSpan timeframe,
    DateTimeOffset from,
    DateTimeOffset to,
    [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
{
    var tf = timeframe.TotalMinutes switch
    {
        1 => "1m",
        5 => "5m",
        15 => "15m",
        60 => "1h",
        240 => "4h",
        1440 => "1d",
        _ => "1m"
    };

    var url = $"{symbol}/candles?interval={tf}&start={from.ToUnixTimeMilliseconds()}&end={to.ToUnixTimeMilliseconds()}";

    using var resp = await _http.GetAsync(url, ct);
    resp.EnsureSuccessStatusCode();

    var doc = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
    if (doc.ValueKind != System.Text.Json.JsonValueKind.Array)
        yield break;

    foreach (var row in doc.EnumerateArray())
    {
        // row: [ timeMs, open, high, low, close, volume ] (values often strings)
        long ms = row[0].ValueKind == System.Text.Json.JsonValueKind.String
            ? long.Parse(row[0].GetString()!, CultureInfo.InvariantCulture)
            : row[0].GetInt64();

        decimal ParseDec(System.Text.Json.JsonElement e) =>
            e.ValueKind == System.Text.Json.JsonValueKind.Number
                ? e.GetDecimal()
                : decimal.Parse(e.GetString() ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);

        yield return new Candle(
            DateTimeOffset.FromUnixTimeMilliseconds(ms),
            ParseDec(row[1]),
            ParseDec(row[2]),
            ParseDec(row[3]),
            ParseDec(row[4]),
            ParseDec(row[5])
        );
    }
}

public async Task<string> PlaceOrder(string symbol, string side, decimal qty, decimal? price = null, CancellationToken ct = default)
    {
        var order = new Dictionary<string, object?>
        {
            ["market"] = symbol,
            ["side"] = side.ToLowerInvariant(),
            ["orderType"] = price is null ? "market" : "limit",
            ["amount"] = qty.ToString()
        };
        if (price is not null) order["price"] = price.ToString();

        var body = System.Text.Json.JsonSerializer.Serialize(order);
        var resp = await SendAsync(HttpMethod.Post, "order", body, auth: true, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        return json; // TODO: parse orderId
    }

    public Task<bool> CancelOrder(string clientOrderId, CancellationToken ct = default)
        => Task.FromResult(true); // stub for now
}
