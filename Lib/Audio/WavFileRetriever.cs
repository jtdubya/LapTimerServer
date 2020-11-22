using System.IO;

namespace LapTimerServer.Lib.Audio
{
    public class WavFileRetriever
    {
        private readonly string _audioFolderName;

        public WavFileRetriever(string audioFolderName)
        {
            _audioFolderName = audioFolderName;
        }

        public string GetSlowFileName()
        {
            return VerifyFileExists(GetFullPathForFile("tooSlow.wav"));
        }

        public string GetZeroPointFileName()
        {
            return VerifyFileExists(GetFullPathForFile("zeroPoint.wav"));
        }

        public string GetFlatFileName()
        {
            return VerifyFileExists(GetFullPathForFile("flat.wav"));
        }

        public string GetFileNameForTwoDigits(int digits)
        {
            return VerifyFileExists(GetFullPathForFile($"{digits}.wav"));
        }

        public string GetFileForHundredthsDigit(int digit)
        {
            return VerifyFileExists(GetFullPathForFile($"o{digit}.wav"));
        }

        private string GetFullPathForFile(string filename)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), _audioFolderName, filename);
        }

        private string VerifyFileExists(string file)
        {
            if (File.Exists(file))
            {
                return file;
            }

            throw new FileNotFoundException(file);
        }
    }
}