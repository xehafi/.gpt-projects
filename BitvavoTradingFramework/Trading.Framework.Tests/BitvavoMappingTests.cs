using System.Text.Json;
using Trading.Framework.Infrastructure.Bitvavo;
using Trading.Framework.Core;
using Xunit;

public class BitvavoMappingTests
{
    [Fact]
    public void MapsTicker()
    {
        var json = """
        {"event":"ticker","market":"BTC-EUR","price":"60000.50","bid":"60000.40","ask":"60000.60","timestamp":1710000000000}
        """;
        using var doc = JsonDocument.Parse(json);
        var msg = doc.RootElement.Deserialize<BitvavoTickerMsg>()!;
        Assert.True(BitvavoMapping.TryToTicker(msg, out Ticker t));
        Assert.Equal("BTC-EUR", t.Market);
        Assert.Equal(60000.50m, t.Price);
        Assert.Equal(60000.40m, t.BestBid);
        Assert.Equal(60000.60m, t.BestAsk);
    }
}
