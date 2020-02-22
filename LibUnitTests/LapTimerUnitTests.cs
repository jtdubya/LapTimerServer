using System;
using Xunit;
using WebAppPrototype.Lib;

namespace WebAppPrototype.LibUnitTests
{
    public class LapTimerUnitTests

    {
        private readonly LapTimer m_lapTimer = new LapTimer(1);

        [Fact]
        public void GetId()
        {
            LapTimer lapTimerZero = new LapTimer(0);
            LapTimer lapTimerOne = new LapTimer(1);
            LapTimer lapTimerMax = new LapTimer(int.MaxValue);

            Assert.Equal(0, lapTimerZero.GetId());
            Assert.Equal(1, lapTimerOne.GetId());
            Assert.Equal(int.MaxValue, lapTimerMax.GetId());
        }

        [Fact]
        public void AddLapAndGetLapCount_ZeroLaps()
        {
            Assert.Equal(0, m_lapTimer.GetLapCount());
        }

        [Fact]
        public void AddLapAndGetLapCount_OneLap()
        {
            m_lapTimer.AddLap(new TimeSpan());

            Assert.Equal(1, m_lapTimer.GetLapCount());
        }

        [Fact]
        public void AddLapAndGetLapCount_ManyLaps()
        {
            int totalLaps = 9999;

            for (int i = 0; i < totalLaps; i++)
            {
                m_lapTimer.AddLap(new TimeSpan());
            }

            Assert.Equal(totalLaps, m_lapTimer.GetLapCount());
        }

        [Fact]
        public void GetTotalTime_NoLaps_ReturnsZero()
        {
            TimeSpan zeroTime = new TimeSpan(0);
            Assert.Equal(zeroTime, m_lapTimer.GetTotalTime());
        }

        [Fact]
        public void GetTotalTime_OneLap()
        {
            int days = 0, hours = 2, minutes = 48, seconds = 14, millis = 74;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis));

            TimeSpan totalTime = m_lapTimer.GetTotalTime();

            Assert.Equal(days, totalTime.Days);
            Assert.Equal(hours, totalTime.Hours);
            Assert.Equal(minutes, totalTime.Minutes);
            Assert.Equal(seconds, totalTime.Seconds);
            Assert.Equal(millis, totalTime.Milliseconds);
        }

        [Fact]
        public void GetTotalTime_ManyLaps()
        {
            int days = 10, hours = 4, minutes = 31, seconds = 56, millis = 860;
            m_lapTimer.AddLap(new TimeSpan(days, 0, 0, 0, 0));
            m_lapTimer.AddLap(new TimeSpan(0, hours, 0, 0, 0));
            m_lapTimer.AddLap(new TimeSpan(0, 0, minutes, 0, 0));
            m_lapTimer.AddLap(new TimeSpan(0, 0, 0, seconds, 0));
            m_lapTimer.AddLap(new TimeSpan(0, 0, 0, 0, millis));

            TimeSpan totalTime = m_lapTimer.GetTotalTime();

            Assert.Equal(days, totalTime.Days);
            Assert.Equal(hours, totalTime.Hours);
            Assert.Equal(minutes, totalTime.Minutes);
            Assert.Equal(seconds, totalTime.Seconds);
            Assert.Equal(millis, totalTime.Milliseconds);
        }

        [Fact]
        public void GetFastestLap_NoLaps_ReturnsLapWithZeros()
        {
            Lap fastestLap = m_lapTimer.GetFastestLap();
            TimeSpan zeroTime = new TimeSpan(0);

            Assert.Equal(0, fastestLap.GetLapNumber());
            Assert.Equal(zeroTime, fastestLap.GetLapTime());
        }

        [Fact]
        public void GetFastestLap_Onelap()
        {
            int days = 0, hours = 0, minutes = 1, seconds = 32, millis = 480;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis));

            Lap fastestlap = m_lapTimer.GetFastestLap();

            Assert.Equal(1, fastestlap.GetLapNumber());
            Assert.Equal(days, fastestlap.GetLapTime().Days);
            Assert.Equal(hours, fastestlap.GetLapTime().Hours);
            Assert.Equal(minutes, fastestlap.GetLapTime().Minutes);
            Assert.Equal(seconds, fastestlap.GetLapTime().Seconds);
            Assert.Equal(millis, fastestlap.GetLapTime().Milliseconds);
        }

        [Fact]
        public void GetFastestLap_ManyLaps_FastestLapIsFirstlap()
        {
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis)); // fastest lap
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 10));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 1, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes + 1, seconds, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 250));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 1));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 30, millis + 600));
            m_lapTimer.AddLap(new TimeSpan(days + 1, hours, minutes, seconds, millis));

            Lap fastestlap = m_lapTimer.GetFastestLap();

            Assert.Equal(1, fastestlap.GetLapNumber());
            Assert.Equal(days, fastestlap.GetLapTime().Days);
            Assert.Equal(hours, fastestlap.GetLapTime().Hours);
            Assert.Equal(minutes, fastestlap.GetLapTime().Minutes);
            Assert.Equal(seconds, fastestlap.GetLapTime().Seconds);
            Assert.Equal(millis, fastestlap.GetLapTime().Milliseconds);
        }

        [Fact]
        public void GetFastestLap_ManyLaps_FastestLapIsMiddlelap()
        {
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 10));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 1, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes + 1, seconds, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 250));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis)); // fastest lap
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 1));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 30, millis + 600));
            m_lapTimer.AddLap(new TimeSpan(days + 1, hours, minutes, seconds, millis));

            Lap fastestlap = m_lapTimer.GetFastestLap();

            Assert.Equal(5, fastestlap.GetLapNumber());
            Assert.Equal(days, fastestlap.GetLapTime().Days);
            Assert.Equal(hours, fastestlap.GetLapTime().Hours);
            Assert.Equal(minutes, fastestlap.GetLapTime().Minutes);
            Assert.Equal(seconds, fastestlap.GetLapTime().Seconds);
            Assert.Equal(millis, fastestlap.GetLapTime().Milliseconds);
        }

        [Fact]
        public void GetFastestLap_ManyLaps_FastestLapIsLastLap()
        {
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 10));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 1, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes + 1, seconds, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 250));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis + 1));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds + 30, millis + 600));
            m_lapTimer.AddLap(new TimeSpan(days + 1, hours, minutes, seconds, millis));
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis)); // fastest lap

            Lap fastestlap = m_lapTimer.GetFastestLap();

            Assert.Equal(8, fastestlap.GetLapNumber());
            Assert.Equal(days, fastestlap.GetLapTime().Days);
            Assert.Equal(hours, fastestlap.GetLapTime().Hours);
            Assert.Equal(minutes, fastestlap.GetLapTime().Minutes);
            Assert.Equal(seconds, fastestlap.GetLapTime().Seconds);
            Assert.Equal(millis, fastestlap.GetLapTime().Milliseconds);
        }
    }
}