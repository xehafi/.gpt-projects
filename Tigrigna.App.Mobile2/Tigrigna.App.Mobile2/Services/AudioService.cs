using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;
using System.Collections.Concurrent;

namespace Tigrigna.App.Mobile2.Services;

public class AudioService : IAudioService
{
    private readonly IAudioManager _audioManager;
    private readonly ConcurrentDictionary<string, byte[]> _cache = new(StringComparer.OrdinalIgnoreCase);

    public AudioService(IAudioManager audioManager) => _audioManager = audioManager;

    public async Task PlayAsync(string assetFileName, string? ttsFallback = null)
    {
        try
        {
            var bytes = await GetBytesAsync(assetFileName);
            if (bytes is null)
            {
                if (!string.IsNullOrWhiteSpace(ttsFallback))
                {
                    try { await TextToSpeech.SpeakAsync(ttsFallback); } catch { /* ignore */ }
                }
                return;
            }

            using var ms = new MemoryStream(bytes);
            using var player = _audioManager.CreatePlayer(ms);
            player.Play();

            // Wait until finished to avoid disposing early
            while (player.IsPlaying)
                await Task.Delay(50);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(ttsFallback))
            {
                try { await TextToSpeech.SpeakAsync(ttsFallback); } catch { /* ignore */ }
            }
        }
    }

    private async Task<byte[]?> GetBytesAsync(string assetFileName)
    {
        if (_cache.TryGetValue(assetFileName, out var found)) return found;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(assetFileName);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            _cache[assetFileName] = bytes;
            return bytes;
        }
        catch
        {
            return null; // not found
        }
    }
}
