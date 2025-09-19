using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Trading.Framework.Core;
using Trading.Framework.Core.Abstractions;

public sealed class TickersPageModel : PageModel
{
    private readonly ITradeRepository _repo;
    public TickersPageModel(ITradeRepository repo) => _repo = repo;

    [BindProperty(SupportsGet = true)] public string? Market { get; set; }
    [BindProperty(SupportsGet = true)] public int Take { get; set; } = 200;

    public List<Ticker> Items { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Items = (await _repo.GetRecentTickersAsync(Market, Take, ct)).ToList();
    }
}
