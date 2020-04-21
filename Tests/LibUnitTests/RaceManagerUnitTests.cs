using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LapTimerServer.Lib;
using Xunit;

namespace LapTimerServer.LibUnitTests
{
    public class RaceManagerUnitTests
    {
        private readonly RaceManager _raceManager;

        public RaceManagerUnitTests()
        {
            _raceManager = new RaceManager();
        }

        [Theory]
        [InlineData(50)]
        [InlineData(1)]
        public void GetSetMaxParticipants(int maxParticipants)
        {
            _raceManager.SetMaxParticipants(maxParticipants);
            Assert.Equal(maxParticipants, _raceManager.GetMaxParticipants());
        }

        [Fact]
        public void Register_IdleStateWithDuplicates_ReturnsRegisteredID()
        {
            int first = _raceManager.Register("10.0.1.1");
            int second = _raceManager.Register("10.0.1.2");
            int secondDuplicate = _raceManager.Register("10.0.1.2");
            int firstDuplicate = _raceManager.Register("10.0.1.1");
            int firstTriplicate = _raceManager.Register("10.0.1.1");

            Assert.Equal(1, first);
            Assert.Equal(1, firstDuplicate);
            Assert.Equal(1, firstTriplicate);
            Assert.Equal(2, second);
            Assert.Equal(2, secondDuplicate);
            Assert.Equal(2, _raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleState()
        {
            _raceManager.SetMaxParticipants(3);
            int first = _raceManager.Register("10.0.1.1");
            int second = _raceManager.Register("10.0.1.2");
            int third = _raceManager.Register("10.0.1.3");

            Assert.Equal(1, first);
            Assert.Equal(2, second);
            Assert.Equal(3, third);
            Assert.Equal(3, _raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleStateWithMoreThanMax_ThrowsException()
        {
            _raceManager.SetMaxParticipants(1);
            int first = _raceManager.Register("10.0.1.1");

            Exception exception = Assert.Throws<InvalidOperationException>(() => _raceManager.Register("10.0.1.2"));

            Assert.Contains("Registration closed. Max participants reached.", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithBadIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => _raceManager.Register("l;sakdjfflsakdjf"));
            Assert.Contains("Could not parse IP address", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithNullIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => _raceManager.Register(null));
            Assert.Contains("Must provide device IP address to register", exception.Message);
        }

        [Fact]
        public void Register_NonIdleState_ThrowException()
        {
            _raceManager.StartRace();
            Exception exception = Assert.Throws<InvalidOperationException>(() => _raceManager.Register("1.1.1.1"));
            Assert.Contains("Registration closed. Wait until next registration period.", exception.Message);
        }

        [Fact]
        public void GetRaceState_InitialState_StateIsIdle()
        {
            Assert.Equal(RaceState.Idle, _raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetRaceState_StateIsWaitingToStart()
        {
            _raceManager.StartRace();
            Assert.Equal(RaceState.StartCountdown, _raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetRaceState_ZeroCountDown_StateIsInProgress()
        {
            _raceManager.RaceStartCountdownDuration = 0;
            _raceManager.StartRace();
            Thread.Sleep(5); // Countdown must go through one loop
            Assert.Equal(RaceState.InProgress, _raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetTimeUntilRaceStart_TimerCountsDownUntilRaceStarts()
        {
            _raceManager.RaceStartCountdownDuration = 15;
            _raceManager.StartRace();
            Thread.Sleep(5);
            long beforeMs = _raceManager.GetMillisecondsUntilRaceStart();
            Thread.Sleep(1);
            long afterMs = _raceManager.GetMillisecondsUntilRaceStart();

            Assert.Equal(RaceState.StartCountdown, _raceManager.GetRaceState());

            while (beforeMs > 0)
            {
                Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
                beforeMs = _raceManager.GetMillisecondsUntilRaceStart();
                Thread.Sleep(1);
                afterMs = _raceManager.GetMillisecondsUntilRaceStart();
            }

            Assert.Equal(RaceState.InProgress, _raceManager.GetRaceState());
        }

        [Fact]
        public void CancelRaceStartCountdown()
        {
            _raceManager.RaceStartCountdownDuration = 1000;
            _raceManager.StartRace();
            Thread.Sleep(5);
            _raceManager.CancelCountdown();
            Thread.Sleep(5);
            Assert.Equal(-1, _raceManager.GetMillisecondsUntilRaceStart());
            Assert.Equal(RaceState.Idle, _raceManager.GetRaceState());
        }

        [Fact]
        public void FinishRace_NoCountdown()
        {
            _raceManager.StartRace(0);
            _raceManager.FinishRace(0);
            Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
        }

        [Fact]
        public void FinishRaceAndGetTimeUntilFinish_UseCountdown()
        {
            _raceManager.StartRace(0);
            _raceManager.FinishRace(15);
            Thread.Sleep(5);
            long beforeMs = _raceManager.GetMillisecondsUntilRaceFinish();
            Thread.Sleep(1);
            long afterMs = _raceManager.GetMillisecondsUntilRaceFinish();
            Assert.Equal(RaceState.FinishCountdown, _raceManager.GetRaceState());

            while (beforeMs > 0)
            {
                Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
                beforeMs = _raceManager.GetMillisecondsUntilRaceFinish();
                Thread.Sleep(1);
                afterMs = _raceManager.GetMillisecondsUntilRaceFinish();
            }

            Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
        }

        [Fact]
        public void AddLapResult_NotRegistered_ThrowsException()
        {
            Assert.Throws<KeyNotFoundException>(() => _raceManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan()));
        }

        [Fact]
        public void AddLapResult_RaceFinishesAfterMaxLapsWithOneTimer()
        {
            int numberOflaps = 5;
            _raceManager.Register("1.1.1.1");
            _raceManager.StartRace(0, numberOflaps);

            for (int i = 0; i < numberOflaps; i++)
            {
                _raceManager.AddLapResult(IPAddress.Parse("1.1.1.1"), new TimeSpan(0, 1, i));
                if (i < numberOflaps - 1)
                {
                    Assert.Equal(RaceState.InProgress, _raceManager.GetRaceState());
                }
            }

            Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
        }

        [Fact]
        public void AddLapResult_RaceFinishesAfterMaxLapsWithMultipleTimers()
        {
            int numberOflaps = 5;
            _raceManager.WaitForAllCarsToFinishDuration = 10000;
            _raceManager.SetMaxParticipants(3);
            var ip1 = IPAddress.Parse("1.1.1.1");
            var ip2 = IPAddress.Parse("2.2.2.2");
            var ip3 = IPAddress.Parse("3.3.3.3");
            _raceManager.Register(ip1.ToString());
            _raceManager.Register(ip2.ToString());
            _raceManager.Register(ip3.ToString());

            _raceManager.StartRace(0, numberOflaps);

            for (int i = 0; i < numberOflaps - 1; i++)
            {
                _raceManager.AddLapResult(ip3, new TimeSpan(0, 1, i - 1));
                _raceManager.AddLapResult(ip2, new TimeSpan(0, 1, i));
                _raceManager.AddLapResult(ip1, new TimeSpan(0, 1, i + 1));
                Assert.Equal(RaceState.InProgress, _raceManager.GetRaceState());
            }

            _raceManager.AddLapResult(ip3, new TimeSpan(0, 1, 1)); // first car finishes
            Assert.Equal(RaceState.FinishCountdown, _raceManager.GetRaceState());
            _raceManager.AddLapResult(ip1, new TimeSpan(0, 1, 2));
            Assert.Equal(RaceState.FinishCountdown, _raceManager.GetRaceState());
            _raceManager.AddLapResult(ip2, new TimeSpan(0, 1, 3)); // last car finishes
            Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
            var finishedIds = _raceManager.GetFinishedParticipantsForLastRace();

            // assert finished order
            Assert.Equal(3, finishedIds[0]);
            Assert.Equal(1, finishedIds[1]);
            Assert.Equal(2, finishedIds[2]);
        }

        [Fact]
        public void GetAllRaces_NoRaces_ReturnsZero()
        {
            Assert.Empty(_raceManager.GetAllRaces());
        }

        [Fact]
        public void GetAllRaces_OneRace()
        {
            _raceManager.RaceStartCountdownDuration = 0;
            _raceManager.StartRace();
            Thread.Sleep(5);
            Assert.Single(_raceManager.GetAllRaces());
        }

        [Fact]
        public void GetAllRaces_ManyRaces()
        {
            int raceCount = 1000;
            for (int i = 0; i < raceCount; i++)
            {
                _raceManager.StartRace(0);
                _raceManager.FinishRace(0);
            }
            Assert.Equal(raceCount, _raceManager.GetAllRaces().Count);
        }
    }
}