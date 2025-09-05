using Microsoft.Maui.Controls;

namespace Tigrinya.App.Mobile.Controls;

public class MiniEthiopicKeyboardView : ContentView
{
    public event Action<string>? KeyTapped;
    public event Action? BackspaceTapped;
    public event Action? ClearTapped;
    public event Action? SpaceTapped;

    public MiniEthiopicKeyboardView()
    {
        var glyphRows = new List<List<string>>
        {
            new() { "ሀ","ሁ","ሂ","ሃ","ሄ","ህ","ሆ" },
            new() { "ለ","ሉ","ሊ","ላ","ሌ","ል","ሎ" },
            new() { "መ","ሙ","ሚ","ማ","ሜ","ም","ሞ" },
        };

        var root = new VerticalStackLayout { Spacing = 8 };

        foreach (var row in glyphRows)
        {
            var flex = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.NoWrap, JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start };
            foreach (var g in row)
            {
                var b = new Button { Text = g, FontSize = 22, WidthRequest = 44, HeightRequest = 44, Margin = new Thickness(4,0) };
                b.Clicked += (s, e) => KeyTapped?.Invoke(g);
                flex.Children.Add(b);
            }
            root.Children.Add(flex);
        }

        var actions = new HorizontalStackLayout { Spacing = 8 };
        var back = new Button { Text = "⌫" };
        back.Clicked += (s, e) => BackspaceTapped?.Invoke();
        var space = new Button { Text = "Space" };
        space.Clicked += (s, e) => SpaceTapped?.Invoke();
        var clear = new Button { Text = "Clear" };
        clear.Clicked += (s, e) => ClearTapped?.Invoke();
        actions.Children.Add(back);
        actions.Children.Add(space);
        actions.Children.Add(clear);
        root.Children.Add(actions);

        Content = root;
    }
}
