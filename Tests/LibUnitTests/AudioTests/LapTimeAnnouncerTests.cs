using Xunit;
using FluentAssertions;
using LapTimerServer.Lib.Audio;

namespace LapTimerServer.Tests.LibUnitTests.AudioTests
{
    public class LapTimeAnnouncerTests
    {
        private const string FolderName = @"Lib\Audio\WavFiles";
        private readonly TestAudioPlayer _audioPlayer;
        private readonly LapTimeAnnouncer _lapTimerAnnouncer;

        public LapTimeAnnouncerTests()
        {
            _audioPlayer = new TestAudioPlayer();
            _lapTimerAnnouncer = new LapTimeAnnouncer(_audioPlayer, FolderName);
        }

        [Theory]
        [InlineData(6.21, "6.wav", "21.wav")]
        [InlineData(1.04, "1.wav", "o4.wav")]
        [InlineData(0.05, "zeroPoint.wav", "o5.wav")]
        [InlineData(6, "6.wav", "flat.wav")]
        public void Announce_TwoFileLapTimes(double lapTime, string firstFileName, string secondFileName)
        {
            _lapTimerAnnouncer.Announce(lapTime);
            _audioPlayer.PlayedFiles.Dequeue().Should().Contain(FolderName).And.EndWith(firstFileName);
            _audioPlayer.PlayedFiles.Dequeue().Should().Contain(FolderName).And.EndWith(secondFileName);
        }

        [Fact]
        public void Announce_TimeIsOverAMinute()
        {
            _lapTimerAnnouncer.Announce(60);
            _audioPlayer.PlayedFiles.Dequeue().Should().Contain(FolderName).And.EndWith("tooSlow.wav");
        }

        [Fact]
        public void Announce_LapTimeIsZero()
        {
            _lapTimerAnnouncer.Announce(0);
            _audioPlayer.PlayedFiles.Should().BeEmpty();
        }
    }
}