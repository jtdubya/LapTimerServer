using System;
using System.Diagnostics;

namespace LapTimerServer.Lib.Audio
{
    public class LinuxWavFilePlayer : IAudioFilePlayer
    {
        private const string CommandPrefix = "-c \"/usr/bin/omxplayer \"";

        public void Play(string filename)
        {
            Play(new string[] { filename });
        }

        public void Play(string[] filenames)
        {
            using Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            for (int i = 0; i < filenames.Length; i++)
            {
                process.StartInfo.Arguments = $"{CommandPrefix}{filenames[i]}";
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                Console.WriteLine("command output:");
                Console.WriteLine(result);
                process.WaitForExit();
            }
        }
    }
}