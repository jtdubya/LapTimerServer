using System;
using System.Collections.Generic;
using WebAppPrototype.Lib;
using Xunit;

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
        public void AddLapAndGetAllLaps_ManyLaps()
        {
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            TimeSpan lap1Time = new TimeSpan(days, hours, minutes, seconds, millis);
            TimeSpan lap2Time = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan lap3Time = new TimeSpan(days, hours, minutes, seconds + 1, millis);
            TimeSpan lap4Time = new TimeSpan(days, hours, minutes + 1, seconds, millis);
            TimeSpan lap5Time = new TimeSpan(days, hours, minutes, seconds, millis + 250);

            m_lapTimer.AddLap(lap1Time);
            m_lapTimer.AddLap(lap2Time);
            m_lapTimer.AddLap(lap3Time);
            m_lapTimer.AddLap(lap4Time);
            m_lapTimer.AddLap(lap5Time);

            List<Lap> allLaps = m_lapTimer.GetAllLaps();

            Assert.Equal(1, allLaps[0].Number);
            Assert.Equal(lap1Time, allLaps[0].Time);

            Assert.Equal(2, allLaps[1].Number);
            Assert.Equal(lap2Time, allLaps[1].Time);

            Assert.Equal(3, allLaps[2].Number);
            Assert.Equal(lap3Time, allLaps[2].Time);

            Assert.Equal(4, allLaps[3].Number);
            Assert.Equal(lap4Time, allLaps[3].Time);

            Assert.Equal(5, allLaps[4].Number);
            Assert.Equal(lap5Time, allLaps[4].Time);
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

            Assert.Equal(0, fastestLap.Number);
            Assert.Equal(zeroTime, fastestLap.Time);
        }

        [Fact]
        public void GetFastestLap_Onelap()
        {
            int days = 0, hours = 0, minutes = 1, seconds = 32, millis = 480;
            m_lapTimer.AddLap(new TimeSpan(days, hours, minutes, seconds, millis));

            Lap fastestlap = m_lapTimer.GetFastestLap();

            Assert.Equal(1, fastestlap.Number);
            Assert.Equal(days, fastestlap.Time.Days);
            Assert.Equal(hours, fastestlap.Time.Hours);
            Assert.Equal(minutes, fastestlap.Time.Minutes);
            Assert.Equal(seconds, fastestlap.Time.Seconds);
            Assert.Equal(millis, fastestlap.Time.Milliseconds);
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

            Assert.Equal(1, fastestlap.Number);
            Assert.Equal(days, fastestlap.Time.Days);
            Assert.Equal(hours, fastestlap.Time.Hours);
            Assert.Equal(minutes, fastestlap.Time.Minutes);
            Assert.Equal(seconds, fastestlap.Time.Seconds);
            Assert.Equal(millis, fastestlap.Time.Milliseconds);
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

            Assert.Equal(5, fastestlap.Number);
            Assert.Equal(days, fastestlap.Time.Days);
            Assert.Equal(hours, fastestlap.Time.Hours);
            Assert.Equal(minutes, fastestlap.Time.Minutes);
            Assert.Equal(seconds, fastestlap.Time.Seconds);
            Assert.Equal(millis, fastestlap.Time.Milliseconds);
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

            Assert.Equal(8, fastestlap.Number);
            Assert.Equal(days, fastestlap.Time.Days);
            Assert.Equal(hours, fastestlap.Time.Hours);
            Assert.Equal(minutes, fastestlap.Time.Minutes);
            Assert.Equal(seconds, fastestlap.Time.Seconds);
            Assert.Equal(millis, fastestlap.Time.Milliseconds);
        }
    }
}