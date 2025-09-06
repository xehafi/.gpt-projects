namespace Tigrinya.App.Mobile.Controls;

public class NumericKeyboardView : ContentView
{
    public event Action<char>? KeyTapped;
    public event Action? BackspaceTapped;
    public event Action? ClearTapped;

    public NumericKeyboardView()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() },
            RowDefinitions = new RowDefinitionCollection { new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition() },
            ColumnSpacing = 8,
            RowSpacing = 8
        };
        string[] keys = { "1","2","3","4","5","6","7","8","9","C","0","⌫" };
        for (int i = 0; i < keys.Length; i++)
        {
            var r = i / 3; var c = i % 3;
            var k = keys[i];
            var b = new Button { Text = k, FontSize = 20 };
            b.Clicked += (s, e) =>
            {
                if (k == "C") ClearTapped?.Invoke();
                else if (k == "⌫") BackspaceTapped?.Invoke();
                else KeyTapped?.Invoke(k[0]);
            };
            grid.Add(b, c, r);
        }
        Content = grid;
    }
}
