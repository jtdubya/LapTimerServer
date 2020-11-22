using LapTimerServer.Lib.Audio;
using System.Collections.Generic;

namespace LapTimerServer.Tests.LibUnitTests.AudioTests
{
    public class TestAudioPlayer : IAudioFilePlayer
    {
        public TestAudioPlayer()
        {
            PlayedFiles = new Queue<string>();
        }

        public void Play(string filename)
        {
            PlayedFiles.Enqueue(filename);
        }

        public void Play(string[] filenames)
        {
            for (int i = 0; i < filenames.Length; i++)
            {
                PlayedFiles.Enqueue(filenames[i]);
            }
        }

        public Queue<string> PlayedFiles { get; }
    }
}