using System;
using System.Globalization;

namespace LapTimerServer.Lib.Audio
{
    public class LapTimeAnnouncer
    {
        private readonly IAudioFilePlayer _audioFilePlayer;
        private readonly WavFileRetriever _wavFileRetriever;

        public LapTimeAnnouncer(IAudioFilePlayer audioFilePlayer, string audioFolderName)
        {
            _audioFilePlayer = audioFilePlayer;
            _wavFileRetriever = new WavFileRetriever(audioFolderName);
        }

        public void Announce(double lapTime)
        {
            string[] audioFiles = GetAudioFilesForLapTime(lapTime);
            _audioFilePlayer.Play(audioFiles);
        }

        private string[] GetAudioFilesForLapTime(double lapTime)
        {
            if (lapTime == 0.0)
            {
                return Array.Empty<string>();
            }
            if (lapTime >= 60.0)
            {
                return new string[] { _wavFileRetriever.GetSlowFileName() };
            }

            string timeString = lapTime.ToString("0.00", CultureInfo.InvariantCulture);
            int seconds = Convert.ToInt32(timeString.Split('.')[0]);
            string subSecondsString = timeString.Split('.')[1];
            int subSeconds = Convert.ToInt32(subSecondsString);

            string secondsFile;
            if (seconds == 0)
            {
                secondsFile = _wavFileRetriever.GetZeroPointFileName();
            }
            else
            {
                secondsFile = _wavFileRetriever.GetFileNameForTwoDigits(seconds);
            }

            string subSecondsFile;
            if (subSeconds == 0)
            {
                subSecondsFile = _wavFileRetriever.GetFlatFileName();
            }
            else if (subSecondsString[0] == '0')
            {
                subSecondsFile = _wavFileRetriever.GetFileForHundredthsDigit(subSeconds);
            }
            else
            {
                subSecondsFile = _wavFileRetriever.GetFileNameForTwoDigits(subSeconds);
            }

            return new string[] { secondsFile, subSecondsFile };
        }
    }
}