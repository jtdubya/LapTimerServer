using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
        public void Register_RegisterState()
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
        public void Register_RegisterStateWithMoreThanMax_ReturnsCode()
        {
            _raceManager.SetMaxParticipants(1);
            _raceManager.Register("10.0.1.1");
            var secondResponse = _raceManager.Register("10.0.1.2");

            Assert.Equal(-2, secondResponse);
        }

        [Fact]
        public void Register_RegisterStateWithBadIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<FormatException>(() => _raceManager.Register("l;sakdjfflsakdjf"));
            Assert.Contains("Could not parse IP address", exception.Message);
        }

        [Fact]
        public void Register_RegisterStateWithNullIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<ArgumentNullException>(() => _raceManager.Register(null));
            Assert.Contains("Must provide device IP address to register", exception.Message);
        }

        [Fact]
        public void Register_NonRegisterState_ReturnsCode()
        {
            _raceManager.StartRace();
            var response = _raceManager.Register("1.1.1.1");
            Assert.Equal(-1, response);
        }

        [Fact]
        public void GetRaceState_InitialState_StateIsIdle()
        {
            Assert.Equal(RaceState.Registration, _raceManager.GetRaceState());
        }

        [Fact]
        public void StartRaceAndGetRaceState_StateIsWaitingToStart()
        {
            _raceManager.StartRace(10000);
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

        // TODO: test fails when ran in parallel
        //[Fact]
        //public void StartRaceAndGetTimeUntilRaceStart_TimerCountsDownUntilRaceStarts()
        //{
        //    _raceManager.RaceStartCountdownDuration = 15;
        //    _raceManager.StartRace();
        //    Thread.Sleep(5);
        //    long beforeMs = _raceManager.GetMillisecondsUntilRaceStart();
        //    Thread.Sleep(1);
        //    long afterMs = _raceManager.GetMillisecondsUntilRaceStart();

        //    Assert.Equal(RaceState.StartCountdown, _raceManager.GetRaceState());

        //    while (beforeMs > 0)
        //    {
        //        Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
        //        beforeMs = _raceManager.GetMillisecondsUntilRaceStart();
        //        Thread.Sleep(1);
        //        afterMs = _raceManager.GetMillisecondsUntilRaceStart();
        //    }

        //    Assert.Equal(RaceState.InProgress, _raceManager.GetRaceState());
        //}

        // TODO: test fails when ran in parallel
        //[Fact]
        //public void CancelRaceStartCountdown()
        //{
        //    _raceManager.RaceStartCountdownDuration = 1000;
        //    _raceManager.StartRace();
        //    Thread.Sleep(5);
        //    _raceManager.CancelCountdown();
        //    Thread.Sleep(5);
        //    Assert.Equal(-1, _raceManager.GetMillisecondsUntilRaceStart());
        //    Assert.Equal(RaceState.Registration, _raceManager.GetRaceState());
        //}

        [Fact]
        public void FinishRace_NoCountdown()
        {
            _raceManager.StartRace(0);
            _raceManager.FinishRace(0);
            Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
        }

        // TODO: test fails when ran in parallel
        //[Fact]
        //public void FinishRaceAndGetTimeUntilFinish_UseCountdown()
        //{
        //    _raceManager.StartRace(0);
        //    _raceManager.FinishRace(15);
        //    Thread.Sleep(5);
        //    long beforeMs = _raceManager.GetMillisecondsUntilRaceFinish();
        //    Thread.Sleep(1);
        //    long afterMs = _raceManager.GetMillisecondsUntilRaceFinish();
        //    Assert.Equal(RaceState.FinishCountdown, _raceManager.GetRaceState());

        //    while (beforeMs > 0)
        //    {
        //        Assert.True(beforeMs > afterMs, "Before: " + beforeMs + ". After: " + afterMs);
        //        beforeMs = _raceManager.GetMillisecondsUntilRaceFinish();
        //        Thread.Sleep(1);
        //        afterMs = _raceManager.GetMillisecondsUntilRaceFinish();
        //    }

        //    Assert.Equal(RaceState.Finished, _raceManager.GetRaceState());
        //}

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
            _raceManager.NumberOfLaps = numberOflaps;
            _raceManager.StartRace(0);

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

            _raceManager.NumberOfLaps = numberOflaps;
            _raceManager.StartRace(0);

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

        [Fact]
        public void GetCurrentRaceResults_OneRace()
        {
            _raceManager.SetMaxParticipants(3);
            var ip1 = IPAddress.Parse("1.1.1.1");
            var ip2 = IPAddress.Parse("2.2.2.2");
            var ip3 = IPAddress.Parse("3.3.3.3");
            _raceManager.Register(ip1.ToString());
            _raceManager.Register(ip2.ToString());
            _raceManager.Register(ip3.ToString());

            int numLaps = 5;
            _raceManager.NumberOfLaps = numLaps;
            _raceManager.StartRace(0);

            for (int i = 0; i < numLaps; i++)
            {
                _raceManager.AddLapResult(ip1, new TimeSpan(0, 1, 1));
                _raceManager.AddLapResult(ip2, new TimeSpan(0, 1, 2));
                _raceManager.AddLapResult(ip3, new TimeSpan(0, 1, 3));
            }

            var raceResults = _raceManager.GetCurrentRaceResults();

            Assert.Contains(1, raceResults.Keys);
            Assert.Contains(2, raceResults.Keys);
            Assert.Contains(3, raceResults.Keys);

            for (int i = 1; i <= 3; i++)
            {
                var resultsFound = raceResults.TryGetValue(i, out List<Lap> timer1Laps);
                Assert.True(resultsFound);

                for (int lap = 0; lap < numLaps; lap++)
                {
                    Assert.Equal(new TimeSpan(0, 1, i), timer1Laps[lap].Time);
                }
            }
        }

        [Fact]
        public void GetCurrentRaceResults_ManyRaces()
        {
            _raceManager.SetMaxParticipants(2);
            var ip1 = IPAddress.Parse("1.1.1.1");
            var ip2 = IPAddress.Parse("2.2.2.2");
            _raceManager.Register(ip1.ToString());
            _raceManager.Register(ip2.ToString());
            int numLaps = 5;
            int numRaces = 5;
            _raceManager.NumberOfLaps = numLaps;

            for (int race = 1; race <= numRaces; race++)
            {
                _raceManager.StartRace(0);

                for (int i = 0; i < numLaps; i++)
                {
                    _raceManager.AddLapResult(ip1, new TimeSpan(0, race, 1));
                    _raceManager.AddLapResult(ip2, new TimeSpan(0, race, 2));
                }
            }

            var raceResults = _raceManager.GetCurrentRaceResults();

            Assert.Contains(1, raceResults.Keys);
            Assert.Contains(2, raceResults.Keys);

            for (int id = 1; id < 3; id++)
            {
                var resultsFound = raceResults.TryGetValue(id, out List<Lap> timer1Laps);
                Assert.True(resultsFound);

                for (int lap = 0; lap < numLaps; lap++)
                {
                    Assert.Equal(new TimeSpan(0, numRaces, id), timer1Laps[lap].Time);
                }
            }
        }
    }
}