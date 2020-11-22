using Xunit;
using FluentAssertions;
using LapTimerServer.Lib.Audio;

namespace LapTimerServer.Tests.LibUnitTests.AudioTests
{
    public class WavFileRetrieverTests
    {
        private readonly WavFileRetriever _wavFileRetriever;

        public WavFileRetrieverTests()
        {
            _wavFileRetriever = new WavFileRetriever(@"Lib\Audio\WavFiles");
        }

        [Theory]

        #region 1-99

        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(20)]
        [InlineData(21)]
        [InlineData(22)]
        [InlineData(23)]
        [InlineData(24)]
        [InlineData(25)]
        [InlineData(26)]
        [InlineData(27)]
        [InlineData(28)]
        [InlineData(29)]
        [InlineData(30)]
        [InlineData(31)]
        [InlineData(32)]
        [InlineData(33)]
        [InlineData(34)]
        [InlineData(35)]
        [InlineData(36)]
        [InlineData(37)]
        [InlineData(38)]
        [InlineData(39)]
        [InlineData(40)]
        [InlineData(41)]
        [InlineData(42)]
        [InlineData(43)]
        [InlineData(44)]
        [InlineData(45)]
        [InlineData(46)]
        [InlineData(47)]
        [InlineData(48)]
        [InlineData(49)]
        [InlineData(50)]
        [InlineData(51)]
        [InlineData(52)]
        [InlineData(53)]
        [InlineData(54)]
        [InlineData(55)]
        [InlineData(56)]
        [InlineData(57)]
        [InlineData(58)]
        [InlineData(59)]
        [InlineData(60)]
        [InlineData(61)]
        [InlineData(62)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(65)]
        [InlineData(66)]
        [InlineData(67)]
        [InlineData(68)]
        [InlineData(69)]
        [InlineData(70)]
        [InlineData(71)]
        [InlineData(72)]
        [InlineData(73)]
        [InlineData(74)]
        [InlineData(75)]
        [InlineData(76)]
        [InlineData(77)]
        [InlineData(78)]
        [InlineData(79)]
        [InlineData(80)]
        [InlineData(81)]
        [InlineData(82)]
        [InlineData(83)]
        [InlineData(84)]
        [InlineData(85)]
        [InlineData(86)]
        [InlineData(87)]
        [InlineData(88)]
        [InlineData(89)]
        [InlineData(90)]
        [InlineData(91)]
        [InlineData(92)]
        [InlineData(93)]
        [InlineData(94)]
        [InlineData(95)]
        [InlineData(96)]
        [InlineData(97)]
        [InlineData(98)]
        [InlineData(99)]

        #endregion 1-99

        public void GetFileNameForTwoDigits_AllPossibleDigits(int digits)
        {
            _wavFileRetriever.GetFileNameForTwoDigits(digits).Should().EndWith($"{digits}.wav");
        }

        [Theory]

        #region 1 - 9

        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]

        #endregion 1 - 9

        public void GetFileForHundredthsDigit_AllPossibleDigits(int digit)
        {
            _wavFileRetriever.GetFileForHundredthsDigit(digit).Should().EndWith($"o{digit}.wav");
        }
    }
}