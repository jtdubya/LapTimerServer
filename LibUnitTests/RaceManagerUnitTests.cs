using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebAppPrototype.Lib;
using Xunit;

namespace WebAppPrototype.LibUnitTests
{
    public class RaceManagerUnitTests
    {
        private RaceManager m_raceManager;

        public RaceManagerUnitTests()
        {
            m_raceManager = new RaceManager();
        }

        [Theory]
        [InlineData(50)]
        [InlineData(1)]
        public void GetSetMaxParticipants(int maxParticipants)
        {
            m_raceManager.SetMaxParticipants(maxParticipants);
            Assert.Equal(maxParticipants, m_raceManager.GetMaxParticipants());
        }

        [Fact]
        public void Register_IdleStateWithDuplicates_ReturnsRegisteredID()
        {
            int first = m_raceManager.Register("10.0.1.1");
            int second = m_raceManager.Register("10.0.1.2");
            int secondDuplicate = m_raceManager.Register("10.0.1.2");
            int firstDuplicate = m_raceManager.Register("10.0.1.1");
            int firstTriplicate = m_raceManager.Register("10.0.1.1");

            Assert.Equal(1, first);
            Assert.Equal(1, firstDuplicate);
            Assert.Equal(1, firstTriplicate);
            Assert.Equal(2, second);
            Assert.Equal(2, secondDuplicate);
            Assert.Equal(2, m_raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleState()
        {
            m_raceManager.SetMaxParticipants(3);
            int first = m_raceManager.Register("10.0.1.1");
            int second = m_raceManager.Register("10.0.1.2");
            int third = m_raceManager.Register("10.0.1.3");

            Assert.Equal(1, first);
            Assert.Equal(2, second);
            Assert.Equal(3, third);
            Assert.Equal(3, m_raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleStateWithMoreThanMax_ThrowsException()
        {
            m_raceManager.SetMaxParticipants(1);
            int first = m_raceManager.Register("10.0.1.1");

            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("10.0.1.2"));

            Assert.Contains("Registration closed. Max participants reached.", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithBadIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("l;sakdjfflsakdjf"));
            Assert.Contains("Could not parse IP address", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithNullIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register(null));
            Assert.Contains("Must provide device IP address to register", exception.Message);
        }

        [Fact]
        public void Register_NonIdleState_ThrowException()
        {
            m_raceManager.StartRace();
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("1.1.1.1"));
            Assert.Contains("Registration closed. Wait until next registration period.", exception.Message);
        }

        [Fact]
        public void GetRaceState_InitialState_StateIsIdle()
        {
            Assert.Equal(RaceState.Idle, m_raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetRaceState_StateIsWaitingToStart()
        {
            m_raceManager.StartRace();
            Assert.Equal(RaceState.StartCountdown, m_raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetRaceState_ZeroCountDown_StateIsInProgress()
        {
            m_raceManager.RaceStartCountdownDuration = 0;
            m_raceManager.StartRace();
            Thread.Sleep(5); // Countdown must go through one loop
            Assert.Equal(RaceState.InProgress, m_raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetTimeUntilRaceStart_TimerCountsDownUntilRaceStarts()
        {
            m_raceManager.RaceStartCountdownDuration = 15;
            m_raceManager.StartRace();
            Thread.Sleep(5);
            long beforeMs = m_raceManager.GetMillisecondsUntilRaceStart();
            Thread.Sleep(1);
            long afterMs = m_raceManager.GetMillisecondsUntilRaceStart();

            Assert.Equal(RaceState.StartCountdown, m_raceManager.GetRaceState());

            while (beforeMs > 0)
            {
                Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
                beforeMs = m_raceManager.GetMillisecondsUntilRaceStart();
                Thread.Sleep(1);
                afterMs = m_raceManager.GetMillisecondsUntilRaceStart();
            }

            Assert.Equal(RaceState.InProgress, m_raceManager.GetRaceState());
        }

        [Fact]
        public void CancelRaceStartCountdown()
        {
            m_raceManager.RaceStartCountdownDuration = 1000;
            m_raceManager.StartRace();
            Thread.Sleep(5);
            m_raceManager.CancelCountdown();
            Thread.Sleep(5);
            Assert.Equal(-1, m_raceManager.GetMillisecondsUntilRaceStart());
            Assert.Equal(RaceState.Idle, m_raceManager.GetRaceState());
        }

        [Fact]
        public void FinishRace_NoCountdown()
        {
            m_raceManager.StartRace(0);
            m_raceManager.FinishRace(0);
            Assert.Equal(RaceState.Finished, m_raceManager.GetRaceState());
        }

        [Fact]
        public void FinishRaceAndGetTimeUntilFinish_UseCountdown()
        {
            m_raceManager.StartRace(0);
            m_raceManager.FinishRace(15);
            Thread.Sleep(5);
            long beforeMs = m_raceManager.GetMillisecondsUntilRaceFinish();
            Thread.Sleep(1);
            long afterMs = m_raceManager.GetMillisecondsUntilRaceFinish();
            Assert.Equal(RaceState.FinishCountdown, m_raceManager.GetRaceState());

            while (beforeMs > 0)
            {
                Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
                beforeMs = m_raceManager.GetMillisecondsUntilRaceFinish();
                Thread.Sleep(1);
                afterMs = m_raceManager.GetMillisecondsUntilRaceFinish();
            }

            Assert.Equal(RaceState.Finished, m_raceManager.GetRaceState());
        }

        [Fact]
        public void AddLapResult_NotRegistered_ThrowsException()
        {
            Assert.Throws<KeyNotFoundException>(() => m_raceManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan()));
        }

        [Fact]
        public void AddLapResult_RaceFinishesAfterMaxLapsWithOneTimer()
        {
            int numberOflaps = 5;
            m_raceManager.Register("1.1.1.1");
            m_raceManager.StartRace(0, numberOflaps);

            for (int i = 0; i < numberOflaps; i++)
            {
                m_raceManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan(0, 1, i));
                if (i < numberOflaps - 1)
                {
                    Assert.Equal(RaceState.InProgress, m_raceManager.GetRaceState());
                }
            }

            Assert.Equal(RaceState.Finished, m_raceManager.GetRaceState());
        }

        [Fact]
        public void AddLapResult_RaceFinishesAfterMaxLapsWithMultipleTimers()
        {
            int numberOflaps = 5;
            m_raceManager.WaitForAllCarsToFinishDuration = 10000;
            m_raceManager.SetMaxParticipants(3);
            var ip1 = IPAddress.Parse("1.1.1.1");
            var ip2 = IPAddress.Parse("2.2.2.2");
            var ip3 = IPAddress.Parse("3.3.3.3");
            m_raceManager.Register(ip1.ToString());
            m_raceManager.Register(ip2.ToString());
            m_raceManager.Register(ip3.ToString());

            m_raceManager.StartRace(0, numberOflaps);

            for (int i = 0; i < numberOflaps - 1; i++)
            {
                m_raceManager.AddLapResult(ip3, new TimeSpan(0, 1, i - 1));
                m_raceManager.AddLapResult(ip2, new TimeSpan(0, 1, i));
                m_raceManager.AddLapResult(ip1, new TimeSpan(0, 1, i + 1));
                Assert.Equal(RaceState.InProgress, m_raceManager.GetRaceState());
            }

            m_raceManager.AddLapResult(ip3, new TimeSpan(0, 1, 1)); // first car finishes
            Assert.Equal(RaceState.FinishCountdown, m_raceManager.GetRaceState());
            m_raceManager.AddLapResult(ip1, new TimeSpan(0, 1, 2));
            Assert.Equal(RaceState.FinishCountdown, m_raceManager.GetRaceState());
            m_raceManager.AddLapResult(ip2, new TimeSpan(0, 1, 3)); // last car finishes
            Assert.Equal(RaceState.Finished, m_raceManager.GetRaceState());
            var finishedIds = m_raceManager.GetFinishedParticipantsForLastRace();

            // assert finished order
            Assert.Equal(3, finishedIds[0]);
            Assert.Equal(1, finishedIds[1]);
            Assert.Equal(2, finishedIds[2]);
        }

        [Fact]
        public void GetAllRaces_NoRaces_ReturnsZero()
        {
            Assert.Empty(m_raceManager.GetAllRaces());
        }

        [Fact]
        public void GetAllRaces_OneRace()
        {
            m_raceManager.RaceStartCountdownDuration = 0;
            m_raceManager.StartRace();
            Thread.Sleep(5);
            Assert.Single(m_raceManager.GetAllRaces());
        }

        [Fact]
        public void GetAllRaces_ManyRaces()
        {
            int raceCount = 1000;
            for (int i = 0; i < raceCount; i++)
            {
                m_raceManager.StartRace(0);
                m_raceManager.FinishRace(0);
            }
            Assert.Equal(raceCount, m_raceManager.GetAllRaces().Count);
        }
    }
}