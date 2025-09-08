using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using Tigrigna.App.Mobile2.Views;

namespace Tigrigna.App.Mobile2;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("lesson", typeof(LessonPage));
        Routing.RegisterRoute("lettersType", typeof(LettersTypePage));
        Routing.RegisterRoute("lettersTrace", typeof(LettersTracePage));


        // Find the TabBar
        var tabBar = this.Items.OfType<TabBar>().FirstOrDefault();
        if (tabBar is null) return;

        // Shell structure: TabBar -> ShellSection -> ShellContent
        var contents = tabBar.Items
                             .SelectMany(section => section.Items)     // ShellSection -> ShellContent
                             .OfType<ShellContent>()
                             .ToList();

        SetContent(contents, "Learn", Ioc.Default.GetRequiredService<LearnPage>());
        SetContent(contents, "Progress", Ioc.Default.GetRequiredService<ProgressPage>());
        SetContent(contents, "Settings", Ioc.Default.GetRequiredService<SettingsPage>());
    }

    private static void SetContent(
        List<ShellContent> contents, string title, Page page)
    {
        var sc = contents.FirstOrDefault(c =>
            string.Equals(c.Title, title, StringComparison.OrdinalIgnoreCase));
        if (sc != null)
            sc.Content = page;
        // else: if the ShellContent isn't defined in XAML, we silently skip.
        // (You can add it in XAML; see below.)
    }
}
