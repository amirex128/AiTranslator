using System.Media;
using System.Runtime.InteropServices;

namespace AiTranslator.Services;

public class AudioPlayer : IDisposable
{
    private readonly ILoggingService _loggingService;
    private CancellationTokenSource? _playbackCancellation;
    private bool _isPlaying;

    // Windows Media Control Interface (MCI) for audio playback
    [DllImport("winmm.dll")]
    private static extern int mciSendString(string command, System.Text.StringBuilder returnValue, int returnLength, IntPtr winHandle);

    public AudioPlayer(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task PlayAsync(string audioFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            Stop();
            _playbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _isPlaying = true;

            _loggingService.LogInformation($"Playing audio in background: {audioFilePath}");

            // Determine file type and play accordingly
            var extension = Path.GetExtension(audioFilePath).ToLower();

            if (extension == ".wav")
            {
                await PlayWavAsync(audioFilePath, _playbackCancellation.Token);
            }
            else
            {
                await PlayWithMciAsync(audioFilePath, _playbackCancellation.Token);
            }

            _loggingService.LogInformation("Audio playback completed");
        }
        catch (OperationCanceledException)
        {
            _loggingService.LogInformation("Audio playback cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Error playing audio", ex);
            throw;
        }
        finally
        {
            _isPlaying = false;
            _playbackCancellation?.Dispose();
            _playbackCancellation = null;
        }
    }

    private async Task PlayWavAsync(string audioFilePath, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            using var player = new SoundPlayer(audioFilePath);
            player.PlaySync();
        }, cancellationToken);
    }

    private async Task PlayWithMciAsync(string audioFilePath, CancellationToken cancellationToken)
    {
        var alias = $"audio_{Guid.NewGuid():N}";
        
        try
        {
            // Open the audio file
            var openCommand = $"open \"{audioFilePath}\" type mpegvideo alias {alias}";
            var result = mciSendString(openCommand, null, 0, IntPtr.Zero);
            
            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to open audio file. MCI error code: {result}");
            }

            // Play the audio
            var playCommand = $"play {alias}";
            result = mciSendString(playCommand, null, 0, IntPtr.Zero);
            
            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to play audio. MCI error code: {result}");
            }

            // Wait for playback to complete or cancellation
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var statusBuffer = new System.Text.StringBuilder(128);
                    var statusCommand = $"status {alias} mode";
                    mciSendString(statusCommand, statusBuffer, statusBuffer.Capacity, IntPtr.Zero);
                    
                    var status = statusBuffer.ToString().Trim();
                    if (status == "stopped")
                    {
                        break;
                    }

                    await Task.Delay(100, cancellationToken);
                }
            }, cancellationToken);
        }
        finally
        {
            // Close the audio file
            var closeCommand = $"close {alias}";
            mciSendString(closeCommand, null, 0, IntPtr.Zero);
        }
    }

    public void Stop()
    {
        if (_isPlaying && _playbackCancellation != null && !_playbackCancellation.IsCancellationRequested)
        {
            _playbackCancellation.Cancel();
            _loggingService.LogInformation("Audio playback stopped");
        }
    }

    public void Dispose()
    {
        Stop();
        _playbackCancellation?.Dispose();
    }
}
