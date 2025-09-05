using SkiaSharp;

namespace Tigrinya.App.Mobile.Services;

public class TemplateService
{
    // Map a few glyphs to template IDs (expand later)
    private readonly Dictionary<string, string> _glyphMap = new()
    {
        { "ሀ", "ha" }
    };

    public SKBitmap? LoadTemplateBitmap(string glyph, int size = 256, int strokePx = 26)
    {
        if (!_glyphMap.TryGetValue(glyph, out var id)) return null;
        var path = $"templates/ethiopic/{id}.json";
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(path).Result;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var tpl = System.Text.Json.JsonSerializer.Deserialize<GlyphTemplate>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (tpl == null) return null;
            return Rasterize(tpl, size, strokePx);
        }
        catch
        {
            return null;
        }
    }

    private SKBitmap Rasterize(GlyphTemplate tpl, int size, int strokePx)
    {
        var bmp = new SKBitmap(size, size, true);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokePx,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        foreach (var stroke in tpl.Strokes)
        {
            for (int i = 1; i < stroke.Count; i++)
            {
                var a = stroke[i - 1];
                var b = stroke[i];
                var p1 = new SKPoint(a.X * size, a.Y * size);
                var p2 = new SKPoint(b.X * size, b.Y * size);
                canvas.DrawLine(p1, p2, paint);
            }
        }

        canvas.Flush();
        return bmp;
    }
}

public class GlyphTemplate
{
    public string Glyph { get; set; } = string.Empty;
    public List<List<GlyphPoint>> Strokes { get; set; } = new();
}

public class GlyphPoint
{
    public float X { get; set; }
    public float Y { get; set; }
}
