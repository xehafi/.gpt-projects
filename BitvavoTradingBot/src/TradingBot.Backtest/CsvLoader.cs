using TradingBot.Core.Models;

namespace TradingBot.Backtest;

public static class CsvLoader
{
    // Expects header: timestamp,open,high,low,close,volume
    public static IEnumerable<Candle> Load(string filePath)
    {
        using var sr = new StreamReader(filePath);
        string? line = sr.ReadLine(); // header
        if (line == null) yield break;
        while ((line = sr.ReadLine()) != null)
        {
            var parts = line.Split(',');
            if (parts.Length < 6) continue;
            var t = DateTimeOffset.Parse(parts[0]);
            var o = decimal.Parse(parts[1]);
            var h = decimal.Parse(parts[2]);
            var l = decimal.Parse(parts[3]);
            var c = decimal.Parse(parts[4]);
            var v = decimal.Parse(parts[5]);
            yield return new Candle(t, o, h, l, c, v);
        }
    }
}
