using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LapTimerServer.Lib
{
    public enum RaceState
    {
        Registration,
        StartCountdown,
        InProgress,
        FinishCountdown,
        Finished
    }

    /// <summary>
    /// Manages execution state for lap timers and races
    /// Has a 1:1 relationship to a race track
    /// </summary>
    public class RaceManager
    {
        private int _maxParticipants;
        private long _milliSecondsUntilRaceStart;
        private long _milliSecondsUntilRaceFinish;
        private readonly List<Race> _races;
        private RaceState _raceState;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly LapTimerManager _lapTimerManager;

        public int NumberOfLaps { get; set; } = 10;
        public long RaceStartCountdownDuration { get; set; }
        public long WaitForAllCarsToFinishDuration { get; set; }

        public RaceManager()
        {
            _maxParticipants = 2;
            _raceState = RaceState.Registration;
            _races = new List<Race>();
            _milliSecondsUntilRaceStart = -1;
            _milliSecondsUntilRaceFinish = -1;
            _lapTimerManager = new LapTimerManager(); // could change this to DI
            RaceStartCountdownDuration = 20000;
            WaitForAllCarsToFinishDuration = 10000;
        }

        #region public methods

        public RaceState GetRaceState()
        {
            return _raceState;
        }

        public int GetMaxParticipants()
        {
            return _maxParticipants;
        }

        public void SetMaxParticipants(int newMax)
        {
            _maxParticipants = newMax;
        }

        public List<Race> GetAllRaces()
        {
            return _races;
        }

        /// <summary>
        /// Register a lap timer
        /// </summary>
        /// <param name="ipAddressString"></param>
        /// <returns>
        /// Either ID if successful or a negative code
        /// Codes:
        /// -1 = Registration closed (not in registration state)
        /// -2 = Max participants reached
        /// </returns>
        public int Register(string ipAddressString)
        {
            int returnIdOrCode = -1;

            if (_raceState == RaceState.Registration)
            {
                try
                {
                    IPAddress ipAddress;
                    ipAddress = IPAddress.Parse(ipAddressString);
                    LapTimer lapTimer = _lapTimerManager.GetLapTimerByIPAddress(ipAddress);

                    if (lapTimer == null) // not registered
                    {
                        if (_lapTimerManager.GetAllLapTimers().Count >= _maxParticipants)
                        {
                            returnIdOrCode = -2;
                        }
                        else
                        {
                            returnIdOrCode = _lapTimerManager.RegisterLapTimer(ipAddress);
                        }
                    }
                    else
                    {
                        returnIdOrCode = lapTimer.GetId();
                    }
                }
                catch (FormatException)
                {
                    throw new FormatException("Could not parse IP address.");
                }
                catch (ArgumentNullException)
                {
                    throw new ArgumentNullException("Must provide device IP address to register.");
                }
            }

            return returnIdOrCode;
        }

        public Dictionary<IPAddress, LapTimer> GetParticipants()
        {
            return _lapTimerManager.GetAllLapTimers();
        }

        public void StartRace()
        {
            StartRace(RaceStartCountdownDuration);
        }

        /// <summary>
        /// Can start race from any state except InProgress or FinishCountdown
        /// </summary>
        /// <param name="countDownDuration"></param>
        public void StartRace(long countDownDuration)
        {
            if (_raceState != RaceState.InProgress || _raceState != RaceState.FinishCountdown)
            {
                if (countDownDuration > 0)
                {
                    _raceState = RaceState.StartCountdown;
                    CountdownToRaceStage(RaceState.StartCountdown, countDownDuration);
                }
                else
                {
                    Race race = new Race(NumberOfLaps);
                    var allTimers = _lapTimerManager.GetAllLapTimers();
                    foreach (var timer in allTimers)
                    {
                        race.AddParticipant(timer.Value.GetId());
                    }
                    race.Start();
                    _races.Add(race);
                    _raceState = RaceState.InProgress;
                    _milliSecondsUntilRaceStart = 0;
                }
            }
        }

        public long GetMillisecondsUntilRaceStart()
        {
            return _milliSecondsUntilRaceStart;
        }

        public long GetMillisecondsUntilRaceFinish()
        {
            return _milliSecondsUntilRaceFinish;
        }

        // same cancellation token is shared between start and finishing since you can't do both at the same time

        public void CancelCountdown()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        // Finish countdown is triggered by first result
        // The race is finished after either
        //      1. All participants finish
        //      2. The finish count down expires
        // In case 1, the race duration ends with the last participant adding their result
        // In case 2, the race duration ends with the last car that added a result before the countdown expired
        public void AddLapResult(IPAddress ipAddress, TimeSpan lapTime) // assuming all lap times are sent in order for now
        {
            Lap addedLap = _lapTimerManager.AddLapResult(ipAddress, lapTime);
            int id = _lapTimerManager.GetLapTimerByIPAddress(ipAddress).GetId();

            if (_raceState != RaceState.Registration && _raceState != RaceState.StartCountdown)
            {
                _races.Last().AddLapResult(id, addedLap);
            }

            if (_raceState == RaceState.InProgress)
            {
                if (_races.Last().HasAnyParticipantFinished())
                {
                    if (_lapTimerManager.GetAllLapTimers().Count == 1)
                    {
                        FinishRace(0);
                    }
                    else
                    {
                        FinishRace();
                    }
                }
            }
            else if (_raceState == RaceState.FinishCountdown)
            {
                if (_races.Last().HaveAllParticipantsFinished())
                {
                    CancelCountdown();
                    FinishRace(0);
                }
            }
        }

        /// <summary>
        /// Races can be finished in 3 ways:
        ///     1. Countdown to race finish is triggered by the first car to add results (Default)
        ///     2. Countdown to race finish is triggered manually
        ///     3. Race is ended immediately (can be trigger manually or automatically after all participants have added results)
        /// </summary>
        ///
        public void FinishRace()
        {
            FinishRace(WaitForAllCarsToFinishDuration);
        }

        public void FinishRace(long countDownMilliseconds)
        {
            if (countDownMilliseconds > 0)
            {
                _raceState = RaceState.FinishCountdown;
                CountdownToRaceStage(RaceState.FinishCountdown, countDownMilliseconds);
            }
            else
            {
                _races.Last().Finish();
                _raceState = RaceState.Finished;
                _milliSecondsUntilRaceFinish = 0;
            }
        }

        public List<int> GetFinishedParticipantsForLastRace()
        {
            return _races.Last().GetFinishedParticipants();
        }

        public Dictionary<int, List<Lap>> GetCurrentRaceResults()
        {
            Dictionary<int, List<Lap>> currentResults = new Dictionary<int, List<Lap>>();

            if (_races.Count > 0)
            {
                currentResults = _races.Last().GetResults();
            }
            return currentResults;
        }

        #endregion public methods

        #region private methods

        // made this non-static to use member fields, but we also need to make sure there is only one...
        private Task CountdownToRaceStage(RaceState state, long countDownDuration)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            return Task.Run(() =>
            {
                Stopwatch countDownStopwatch = new Stopwatch();
                countDownStopwatch.Start();

                while (countDownStopwatch.ElapsedMilliseconds < countDownDuration)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (state == RaceState.StartCountdown)
                        {
                            _raceState = RaceState.Registration;
                            _milliSecondsUntilRaceStart = -1;
                        }
                        else if (state == RaceState.FinishCountdown)
                        {
                            _raceState = RaceState.Finished;
                            _milliSecondsUntilRaceFinish = -1;
                        }

                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }

                    // update timers
                    if (state == RaceState.StartCountdown)
                    {
                        _milliSecondsUntilRaceStart = countDownDuration - countDownStopwatch.ElapsedMilliseconds;
                    }
                    else if (state == RaceState.FinishCountdown)
                    {
                        _milliSecondsUntilRaceFinish = countDownDuration - countDownStopwatch.ElapsedMilliseconds;
                    }
                }

                // Countdown completed
                if (state == RaceState.StartCountdown)
                {
                    StartRace(0);
                }
                else if (state == RaceState.FinishCountdown)
                {
                    FinishRace(0);
                }
                countDownStopwatch.Stop();
            }, _cancellationTokenSource.Token);
        }

        #endregion private methods
    }
}