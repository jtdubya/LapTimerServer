using System;
using System.Collections.Generic;
using System.Net;
using LapTimerServer.Lib;
using Xunit;

namespace LapTimerServer.LibUnitTests
{
    public class LapTimerManagerUnitTests
    {
        private readonly LapTimerManager _lapTimerManager = new LapTimerManager();

        [Fact]
        public void RegisterLapTimer_OneLapTimer()
        {
            IPAddress lapTimerIpAddress = IPAddress.Parse("10.0.1.1");
            int id = _lapTimerManager.RegisterLapTimer(lapTimerIpAddress);
            Assert.Equal(1, id);
        }

        [Fact]
        public void RegisterLapTimer_ManyLapTimers()
        {
            for (int i = 1; i <= 10; i++)
            {
                string ipAddress = "10.0.1." + i;
                int id = _lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
                Assert.Equal(i, id);
            }
        }

        [Fact]
        public void RegisterLapTimer_TimersWithDuplicateIP_ReturnsId()
        {
            IPAddress lapTimerIpAddress = IPAddress.Parse("10.0.1.1");
            int idOne = _lapTimerManager.RegisterLapTimer(lapTimerIpAddress);
            int idTwo = _lapTimerManager.RegisterLapTimer(lapTimerIpAddress);

            Assert.Equal(1, idOne);
            Assert.Equal(1, idTwo);
        }

        [Fact]
        public void GetAllLapTimers_ZeroTimers()
        {
            Dictionary<IPAddress, LapTimer> timers = _lapTimerManager.GetAllLapTimers();
            Assert.Empty(timers);
        }

        [Fact]
        public void GetAllLapTimers_OneTimers()
        {
            _lapTimerManager.RegisterLapTimer(IPAddress.Parse("10.0.1.1"));
            var timers = _lapTimerManager.GetAllLapTimers();
            Assert.Single(timers);
        }

        [Fact]
        public void GetAllLapTimers_ManyTimers()
        {
            int timerCount = 5;

            for (int i = 1; i <= timerCount; i++)
            {
                string ipAddress = "10.0.1." + i;
                _lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
            }

            // try again with duplicates
            for (int i = 1; i <= timerCount; i++)
            {
                string ipAddress = "10.0.1." + i;
                _lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
            }

            var timers = _lapTimerManager.GetAllLapTimers();

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
            Assert.Throws<KeyNotFoundException>(() => _lapTimerManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan()));
        }

        [Fact]
        public void AddLapResult_OneTimerAndOneResult()
        {
            IPAddress address = IPAddress.Parse("1.1.1.1");
            _lapTimerManager.RegisterLapTimer(address);
            var time = new TimeSpan(0, 1, 20);
            _lapTimerManager.AddLapResult(address, time);

            LapTimer timer = _lapTimerManager.GetLapTimerByIPAddress(address);
            Assert.NotNull(timer);
            var addedLaps = timer.GetAllLaps();

            Assert.Single(addedLaps);
            Assert.Equal(time, addedLaps[0].Time);
        }

        [Fact]
        public void AddLapResult_OneTimerAndManyResults()
        {
            IPAddress address = IPAddress.Parse("1.1.1.1");
            _lapTimerManager.RegisterLapTimer(address);
            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            TimeSpan time1 = new TimeSpan(days, hours, minutes, seconds, millis);
            TimeSpan time2 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan time3 = new TimeSpan(days, hours, minutes, seconds + 1, millis);

            _lapTimerManager.AddLapResult(address, time1);
            _lapTimerManager.AddLapResult(address, time2);
            _lapTimerManager.AddLapResult(address, time3);

            LapTimer timer = _lapTimerManager.GetLapTimerByIPAddress(address);
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
            _lapTimerManager.RegisterLapTimer(timer1Address);
            _lapTimerManager.RegisterLapTimer(timer2Address);

            int days = 0, hours = 0, minutes = 3, seconds = 14, millis = 791;
            TimeSpan timer1lap1 = new TimeSpan(days, hours, minutes, seconds, millis);
            TimeSpan timer1lap2 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan timer1lap3 = new TimeSpan(days, hours, minutes, seconds + 1, millis);

            TimeSpan timer2lap1 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan timer2lap2 = new TimeSpan(days, hours, minutes, seconds + 1, millis);
            TimeSpan timer2lap3 = new TimeSpan(days, hours, minutes + 1, seconds, millis);

            _lapTimerManager.AddLapResult(timer1Address, timer1lap1);
            _lapTimerManager.AddLapResult(timer2Address, timer2lap1);
            _lapTimerManager.AddLapResult(timer1Address, timer1lap2);
            _lapTimerManager.AddLapResult(timer2Address, timer2lap2);
            _lapTimerManager.AddLapResult(timer1Address, timer1lap3);
            _lapTimerManager.AddLapResult(timer2Address, timer2lap3);

            LapTimer timer1 = _lapTimerManager.GetLapTimerByIPAddress(timer1Address);
            Assert.NotNull(timer1);
            var timer1AddedLaps = timer1.GetAllLaps();

            Assert.Equal(1, timer1AddedLaps[0].Number);
            Assert.Equal(2, timer1AddedLaps[1].Number);
            Assert.Equal(3, timer1AddedLaps[2].Number);
            Assert.Equal(timer1lap1, timer1AddedLaps[0].Time);
            Assert.Equal(timer1lap2, timer1AddedLaps[1].Time);
            Assert.Equal(timer1lap3, timer1AddedLaps[2].Time);

            LapTimer timer2 = _lapTimerManager.GetLapTimerByIPAddress(timer2Address);
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