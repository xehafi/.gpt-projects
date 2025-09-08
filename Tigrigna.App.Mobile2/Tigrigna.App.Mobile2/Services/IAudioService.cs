namespace Tigrigna.App.Mobile2.Services;

public interface IAudioService
{
    /// <summary>
    /// Attempts to play an embedded asset from Resources/Raw. If missing, optionally falls back to TTS.
    /// </summary>
    Task PlayAsync(string assetFileName, string? ttsFallback = null);
}
