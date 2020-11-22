namespace LapTimerServer.Lib.Audio
{
    public interface IAudioFilePlayer
    {
        public void Play(string filename);

        public void Play(string[] filenames);
    }
}