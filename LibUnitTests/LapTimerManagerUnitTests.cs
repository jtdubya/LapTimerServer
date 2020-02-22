using System;
using System.Net;
using System.Collections.Generic;
using Xunit;
using WebAppPrototype.Lib;

namespace WebAppPrototype.LibUnitTests
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
                String ipAddress = "10.0.1." + i;
                int id = m_lapTimerManager.RegisterLapTimer(IPAddress.Parse(ipAddress));
                Assert.Equal(i, id);
            }
        }

        [Fact]
        public void RegisterLapTimer_TimersWithDuplicateIP_ReturnsNegativeOne()
        {
            IPAddress lapTimerIpAddress = IPAddress.Parse("10.0.1.1");
            int idOne = m_lapTimerManager.RegisterLapTimer(lapTimerIpAddress);
            int idTwo = m_lapTimerManager.RegisterLapTimer(lapTimerIpAddress);

            Assert.Equal(1, idOne);
            Assert.Equal(-1, idTwo);
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
    }
}