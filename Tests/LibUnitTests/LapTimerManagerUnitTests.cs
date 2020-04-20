using System;
using System.Collections.Generic;
using System.Net;
using LapTimerServer.Lib;
using Xunit;

namespace LapTimerServer.LibUnitTests
{
    public class LapTimerManagerUnitTests
    {
        private LapTimerManager m_lapTimerManager = new LapTimerManager();

        [Fact]
        public void RegisterLapTimer_OneLapTimer()
        {
            IPAddress lapTimerIpAddress = IPAddress.Parse("10.0.1.1");
            int id = m_lapTimerManager.RegisterLapTimer(lapTimerIpAddress);
            Assert.Equal(1, id);
        }

        [Fact]
        public void RegisterLapTimer_ManyLapTimers()
        {
            for (int i = 1; i <= 10; i++)
            {
                string ipAddress = "10.0.1." + i;
                int id = m_lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
                Assert.Equal(i, id);
            }
        }

        [Fact]
        public void RegisterLapTimer_TimersWithDuplicateIP_ReturnsId()
        {
            IPAddress lapTimerIpAddress = IPAddress.Parse("10.0.1.1");
            int idOne = m_lapTimerManager.RegisterLapTimer(lapTimerIpAddress);
            int idTwo = m_lapTimerManager.RegisterLapTimer(lapTimerIpAddress);

            Assert.Equal(1, idOne);
            Assert.Equal(1, idTwo);
        }

        [Fact]
        public void GetAllLapTimers_ZeroTimers()
        {
            Dictionary<IPAddress, LapTimer> timers = m_lapTimerManager.GetAllLapTimers();
            Assert.Empty(timers);
        }

        [Fact]
        public void GetAllLapTimers_OneTimers()
        {
            m_lapTimerManager.RegisterLapTimer(IPAddress.Parse("10.0.1.1"));
            var timers = m_lapTimerManager.GetAllLapTimers();
            Assert.Single(timers);
        }

        [Fact]
        public void GetAllLapTimers_ManyTimers()
        {
            int timerCount = 5;

            for (int i = 1; i <= timerCount; i++)
            {
                string ipAddress = "10.0.1." + i;
                m_lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
            }

            // try again with duplicates
            for (int i = 1; i <= timerCount; i++)
            {
                string ipAddress = "10.0.1." + i;
                m_lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
            }

            var timers = m_lapTimerManager.GetAllLapTimers();

            Assert.Equal(timerCount, timers.Count);

            for (int i = 1; i <= timerCount; i++)
            {
                string ipAddressString = "10.0.1." + i;
                IPAddress ipAddress = IPAddress.Parse(ipAddressString);
                Assert.Contains(ipAddress, timers.Keys);

                bool getSuccess = timers.TryGetValue(ipAddress, out LapTimer lapTimer);

                Assert.True(getSuccess);
                Assert.Equal(i, lapTimer.GetId());
            }
        }

        [Fact]
        public void AddLapResult_IPAddressNotRegistered_ThrowsException()
        {
            Assert.Throws<KeyNotFoundException>(() => m_lapTimerManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan()));
        }

        [Fact]
        public void AddLapResult_OneTimerAndOneResult()
        {
            IPAddress address = IPAddress.Parse("1.1.1.1");
            m_lapTimerManager.RegisterLapTimer(address);
            var time = new TimeSpan(0, 1, 20);
            m_lapTimerManager.AddLapResult(address, time);

            LapTimer timer = m_lapTimerManager.GetLapTimerByIPAddress(address);
            Assert.NotNull(timer);
            var addedLaps = timer.GetAllLaps();

            Assert.Single(addedLaps);
            Assert.Equal(time, addedLaps[0].Time);
        }

        [Fact]
        public void AddLapResult_OneTimerAndManyResults()
        {
            IPAddress address = IPAddress.Parse("1.1.1.1");
            m_lapTimerManager.RegisterLapTimer(address);
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            TimeSpan time1 = new TimeSpan(days, hours, minutes, seconds, millis);
            TimeSpan time2 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan time3 = new TimeSpan(days, hours, minutes, seconds + 1, millis);

            m_lapTimerManager.AddLapResult(address, time1);
            m_lapTimerManager.AddLapResult(address, time2);
            m_lapTimerManager.AddLapResult(address, time3);

            LapTimer timer = m_lapTimerManager.GetLapTimerByIPAddress(address);
            Assert.NotNull(timer);
            var addedLaps = timer.GetAllLaps();

            Assert.Equal(1, addedLaps[0].Number);
            Assert.Equal(2, addedLaps[1].Number);
            Assert.Equal(3, addedLaps[2].Number);
            Assert.Equal(time1, addedLaps[0].Time);
            Assert.Equal(time2, addedLaps[1].Time);
            Assert.Equal(time3, addedLaps[2].Time);
        }

        [Fact]
        public void AddLapResult_MultipleTimersAndMultipleResults()
        {
            IPAddress timer1Address = IPAddress.Parse("1.1.1.1");
            IPAddress timer2Address = IPAddress.Parse("2.2.2.2");
            m_lapTimerManager.RegisterLapTimer(timer1Address);
            m_lapTimerManager.RegisterLapTimer(timer2Address);

            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            TimeSpan timer1lap1 = new TimeSpan(days, hours, minutes, seconds, millis);
            TimeSpan timer1lap2 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan timer1lap3 = new TimeSpan(days, hours, minutes, seconds + 1, millis);

            TimeSpan timer2lap1 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan timer2lap2 = new TimeSpan(days, hours, minutes, seconds + 1, millis);
            TimeSpan timer2lap3 = new TimeSpan(days, hours, minutes + 1, seconds, millis);

            m_lapTimerManager.AddLapResult(timer1Address, timer1lap1);
            m_lapTimerManager.AddLapResult(timer2Address, timer2lap1);
            m_lapTimerManager.AddLapResult(timer1Address, timer1lap2);
            m_lapTimerManager.AddLapResult(timer2Address, timer2lap2);
            m_lapTimerManager.AddLapResult(timer1Address, timer1lap3);
            m_lapTimerManager.AddLapResult(timer2Address, timer2lap3);

            LapTimer timer1 = m_lapTimerManager.GetLapTimerByIPAddress(timer1Address);
            Assert.NotNull(timer1);
            var timer1AddedLaps = timer1.GetAllLaps();

            Assert.Equal(1, timer1AddedLaps[0].Number);
            Assert.Equal(2, timer1AddedLaps[1].Number);
            Assert.Equal(3, timer1AddedLaps[2].Number);
            Assert.Equal(timer1lap1, timer1AddedLaps[0].Time);
            Assert.Equal(timer1lap2, timer1AddedLaps[1].Time);
            Assert.Equal(timer1lap3, timer1AddedLaps[2].Time);

            LapTimer timer2 = m_lapTimerManager.GetLapTimerByIPAddress(timer2Address);
            Assert.NotNull(timer2);
            var timer2AddedLaps = timer2.GetAllLaps();

            Assert.Equal(1, timer2AddedLaps[0].Number);
            Assert.Equal(2, timer2AddedLaps[1].Number);
            Assert.Equal(3, timer2AddedLaps[2].Number);
            Assert.Equal(timer2lap1, timer2AddedLaps[0].Time);
            Assert.Equal(timer2lap2, timer2AddedLaps[1].Time);
            Assert.Equal(timer2lap3, timer2AddedLaps[2].Time);
        }
    }
}