using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
public sealed class MarketPageModel : PageModel
{
    [BindProperty(SupportsGet = true)] public string Market { get; set; } = "BTC-EUR";
    [BindProperty(SupportsGet = true)] public int Minutes { get; set; } = 60;
    public void OnGet() {}
}