using System.Media;

namespace LapTimerServer.Lib.Audio
{
    public class WindowsWavFilePlayer : IAudioFilePlayer
    {
        public void Play(string filename)
        {
            Play(new string[] { filename });
        }

        public void Play(string[] files)
        {
            using SoundPlayer soundPlayer = new SoundPlayer();

            for (int i = 0; i < files.Length; i++)
            {
                soundPlayer.SoundLocation = files[i];
                soundPlayer.PlaySync();
            }
        }
    }
}