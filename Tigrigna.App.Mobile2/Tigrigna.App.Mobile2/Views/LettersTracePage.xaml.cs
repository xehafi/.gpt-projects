using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Maui.Graphics;
using Tigrigna.App.Mobile2.ViewModels;

namespace Tigrigna.App.Mobile2.Views;

public partial class LettersTracePage : ContentPage
{
    private readonly LettersTraceViewModel _vm;
    private readonly TraceDrawable _drawable = new();

    public LettersTracePage(LettersTraceViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        Canvas.Drawable = _drawable;
        _vm.Completed += async (_, __) =>
        {
            await DisplayAlert("Well done!", "You earned +2 XP", "OK");
            await Shell.Current.GoToAsync("..");
        };
    }

    public LettersTracePage() : this(Ioc.Default.GetRequiredService<LettersTraceViewModel>()) { }

    private void OnStart(object? sender, TouchEventArgs e)
    {
        _drawable.StartStroke(e.Touches[0]);
        Canvas.Invalidate();
    }

    private void OnDrag(object? sender, TouchEventArgs e)
    {
        _drawable.ContinueStroke(e.Touches[0]);
        Canvas.Invalidate();
    }

    private void OnEnd(object? sender, TouchEventArgs e)
    {
        _drawable.EndStroke();
        Canvas.Invalidate();
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _drawable.Clear();
        Canvas.Invalidate();
    }
}

// Simple drawable that records freehand strokes
public class TraceDrawable : IDrawable
{
    private readonly List<List<PointF>> _strokes = new();
    private List<PointF>? _current;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 6;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
                canvas.DrawLine(stroke[i - 1], stroke[i]);
        }

        if (_current is { Count: > 1 })
        {
            for (int i = 1; i < _current.Count; i++)
                canvas.DrawLine(_current[i - 1], _current[i]);
        }
    }

    public void StartStroke(PointF start)
    {
        _current = new List<PointF> { start };
    }

    public void ContinueStroke(PointF pt)
    {
        _current?.Add(pt);
    }

    public void EndStroke()
    {
        if (_current is { Count: > 1 }) _strokes.Add(_current);
        _current = null;
    }

    public void Clear()
    {
        _strokes.Clear();
        _current = null;
    }
}
