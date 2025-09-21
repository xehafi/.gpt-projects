using FluentAssertions; 
using Trading.Framework.Core; 
using Xunit;
public class TickerTests
{
    [Fact] public void Ticker_Creates()
    {
        var t = new Ticker("BTC-EUR", 1, 0.9m, 1.1m, 1, System.DateTimeOffset.UtcNow);
        t.Market.Should().Be("BTC-EUR");
    }

}