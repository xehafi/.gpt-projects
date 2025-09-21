using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;

namespace Trading.Framework.Tests
{

    public sealed class FakeMarketDataSource : IMarketDataSource
    {
        private readonly Channel<Ticker> _ch = Channel.CreateUnbounded<Ticker>();
        public Task ConnectAsync(CancellationToken ct) => Task.CompletedTask;
        public Task SubscribeTickersAsync(IEnumerable<string> _, CancellationToken __) => Task.CompletedTask;
        public async IAsyncEnumerable<Ticker> ReadTickersAsync([EnumeratorCancellation] CancellationToken ct)
        {
            for (var i = 0; i < 3; i++)
            {
                await _ch.Writer.WriteAsync(new Ticker("BTC-EUR", 1 + i, 1 + i, 1 + i, i, DateTime.UtcNow), ct);
            }
            _ch.Writer.TryComplete();
            while (await _ch.Reader.WaitToReadAsync(ct))
                while (_ch.Reader.TryRead(out var t)) yield return t;
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<Ticker> StreamTickersAsync(string market, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
