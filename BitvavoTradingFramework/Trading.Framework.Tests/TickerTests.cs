using Trading.Framework.Core;
using FluentAssertions;
using Xunit;
using System;
public class TickerTests
{
    [Fact]
    public void Ticker_Creates()
    {
        var t = new Ticker("BTC-EUR", 1, 0.9m, 1.1m, 1, DateTimeOffset.UtcNow);
        t.Market.Should().Be("BTC-EUR");
    }
}