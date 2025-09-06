using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace Tigrinya.App.Mobile.Controls;

public class TraceCanvasView : SKCanvasView
{
    private readonly List<List<SKPoint>> _strokes = new();
    private List<SKPoint>? _current;
    public float StrokeLength { get; private set; } = 0f;
    private SKBitmap? _template;
    private int _rasterSize = 256;

    public TraceCanvasView()
    {
        EnableTouchEvents = true;
        this.PaintSurface += OnPaintSurface;
        this.Touch += OnTouch;
    }

    public void Clear()
    {
        _strokes.Clear();
        _current = null;
        StrokeLength = 0f;
        InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = SKColors.Black.WithAlpha(200),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 12,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        foreach (var stroke in _strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
            {
                canvas.DrawLine(stroke[i - 1], stroke[i], paint);
            }
        }
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _current = new List<SKPoint> { e.Location };
                _strokes.Add(_current);
                InvalidateSurface();
                e.Handled = true;
                break;
            case SKTouchAction.Moved:
                if (_current != null)
                {
                    var last = _current[^1];
                    _current.Add(e.Location);
                    StrokeLength += Distance(last, e.Location);
                    InvalidateSurface();
                }
                e.Handled = true;
                break;
            case SKTouchAction.Released:
            case SKTouchAction.Cancelled:
                _current = null;
                e.Handled = true;
                break;
        }
    }

    private static float Distance(SKPoint a, SKPoint b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public void SetTemplate(SKBitmap? bmp, int rasterSize = 256)
    {
        _template = bmp;
        _rasterSize = rasterSize;
    }

    public float ComputeScore()
    {
        if (_template == null) return 0f;
        var user = new SKBitmap(_rasterSize, _rasterSize, true);
        using (var c = new SKCanvas(user))
        using (var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 26, StrokeCap = SKStrokeCap.Round, StrokeJoin = SKStrokeJoin.Round })
        {
            // Scale strokes to bitmap space
            var w = this.CanvasSize.Width; var h = this.CanvasSize.Height;
            float sx = _rasterSize / Math.Max(1f, w);
            float sy = _rasterSize / Math.Max(1f, h);
            foreach (var stroke in _strokes)
            {
                for (int i = 1; i < stroke.Count; i++)
                {
                    var a = new SKPoint(stroke[i - 1].X * sx, stroke[i - 1].Y * sy);
                    var b = new SKPoint(stroke[i].X * sx, stroke[i].Y * sy);
                    c.DrawLine(a, b, paint);
                }
            }
            c.Flush();
        }
        // Compute overlap ratio: overlap / templateArea
        long overlap = 0; long tplArea = 0;
        for (int y = 0; y < _rasterSize; y++)
        {
            for (int x = 0; x < _rasterSize; x++)
            {
                var t = _template.GetPixel(x, y).Alpha;
                if (t > 0) { tplArea++; var u = user.GetPixel(x, y).Alpha; if (u > 0) overlap++; }
            }
        }
        if (tplArea == 0) return 0f;
        return (float)overlap / (float)tplArea;
    }
}
