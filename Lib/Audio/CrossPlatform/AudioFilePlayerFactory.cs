using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace LapTimerServer.Lib.Audio.CrossPlatform
{
    public class AudioFilePlayerFactory
    {
        private readonly ILogger _logger;

        public AudioFilePlayerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IAudioFilePlayer CreateWavPlayer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.LogInformation("Using Windows Audio Player");
                return new WindowsWavFilePlayer();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _logger.LogInformation("Using Linux Audio Player");
                return new LinuxWavFilePlayer();
            }
            throw new NotSupportedException($"No audio file supported for OS '{RuntimeInformation.OSDescription}'");
        }
    }
}