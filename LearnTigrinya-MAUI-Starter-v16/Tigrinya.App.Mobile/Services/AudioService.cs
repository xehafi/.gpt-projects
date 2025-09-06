using Plugin.Maui.Audio;
using System.Reflection;
using Microsoft.Maui.Storage;

namespace Tigrinya.App.Mobile.Services;

public class AudioService
{
    private readonly IAudioManager _audioManager;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task PlayAsync(string? resourcePath, string? ttsFallback = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                // Resource path expected under Resources/Raw/
                var stream = await FileSystem.OpenAppPackageFileAsync(resourcePath);
                var player = _audioManager.CreatePlayer(stream);
                player.Play();
                return;
            }
        }
        catch
        {
            // fall through to TTS
        }

        if (!string.IsNullOrWhiteSpace(ttsFallback))
        {
            await TextToSpeech.Default.SpeakAsync(ttsFallback);
        }
    }
}
