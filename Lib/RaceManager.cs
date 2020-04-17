using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace WebAppPrototype.Lib
{
    public enum RaceState
    {
        Idle, // better name?
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
        private int m_maxParticipants;
        private long m_milliSecondsUntilRaceStart;
        private long m_milliSecondsUntilRaceFinish;
        private List<Race> m_races;
        private RaceState m_raceState;
        private CancellationTokenSource m_cancellationTokenSource;
        private readonly LapTimerManager m_lapTimerManager;

        public long RaceStartCountdownDuration { get; set; }
        public long WaitForAllCarsToFinishDuration { get; set; }

        public RaceManager()
        {
            m_maxParticipants = 2;
            m_raceState = RaceState.Idle;
            m_races = new List<Race>();
            m_milliSecondsUntilRaceStart = -1;
            m_milliSecondsUntilRaceFinish = -1;
            m_lapTimerManager = new LapTimerManager(); // could change this to DI
            RaceStartCountdownDuration = 20000;
        }

        #region public methods

        public RaceState GetRaceState()
        {
            return m_raceState;
        }

        public int GetMaxParticipants()
        {
            return m_maxParticipants;
        }

        public void SetMaxParticipants(int newMax)
        {
            m_maxParticipants = newMax;
        }

        public List<Race> GetAllRaces()
        {
            return m_races;
        }

        public int Register(string ipAddressString)
        {
            int id = -1; // only set to positive number if registration is successful
            string errorMessage = "Registration closed. ";

            if (m_raceState == RaceState.Idle)
            {
                try
                {
                    IPAddress ipAddress;
                    ipAddress = IPAddress.Parse(ipAddressString);
                    LapTimer lapTimer = m_lapTimerManager.GetLapTimerByIPAddress(ipAddress);

                    if (lapTimer == null) // not registered
                    {
                        if (m_lapTimerManager.GetAllLapTimers().Count >= m_maxParticipants)
                        {
                            errorMessage += "Max participants reached.";
                        }
                        else
                        {
                            id = m_lapTimerManager.RegisterLapTimer(ipAddress);
                        }
                    }
                    else
                    {
                        id = lapTimer.GetId();
                    }
                }
                catch (FormatException)
                {
                    errorMessage = "Could not parse IP address";
                }
                catch (ArgumentNullException)
                {
                    errorMessage = "Must provide device IP address to register";
                }
            }
            else
            {
                errorMessage += "Wait until next registration period.";
            }

            if (id == -1)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return id;
        }

        public Dictionary<IPAddress, LapTimer> GetParticipants()
        {
            return m_lapTimerManager.GetAllLapTimers();
        }

        public void StartRace()
        {
            StartRace(RaceStartCountdownDuration);
        }

        public void StartRace(long countDownDuration)
        {
            if (countDownDuration > 0)
            {
                m_raceState = RaceState.StartCountdown;
                CountdownToRaceStage(RaceState.StartCountdown, countDownDuration);
            }
            else
            {
                Race race = new Race(10);
                race.Start();
                m_races.Add(race);
                m_raceState = RaceState.InProgress;
                m_milliSecondsUntilRaceStart = 0;
            }
        }

        public long GetMillisecondsUntilRaceStart()
        {
            return m_milliSecondsUntilRaceStart;
        }

        public long GetMillisecondsUntilRaceFinish()
        {
            return m_milliSecondsUntilRaceFinish;
        }

        // same cancellation token is shared between start and finishing since you can't do both at the same time

        public void CancelCountdown()
        {
            m_cancellationTokenSource.Cancel();
            m_cancellationTokenSource.Dispose();
        }

        // Finish countdown is triggered by first result
        // The race is finished after either
        //      1. All participants finish
        //      2. The finish count down expires
        // In case 1, the race duration ends with the last participant adding their result
        // In case 2, the race duration ends with the last car that added a result before the countdown expired
        // TODO: rethink how to do this since we are added laps incrementally
        // public void AddResult(int id, List<Lap> laps)
        //{
        //    int finishedCount = m_races.Last().GetResults().Count;
        //    if (finishedCount == 0) // first to finish, start countdown
        //    {
        //    }
        //    else if (finishedCount == m_maxParticipants)
        //    {
        //        // cancel countdown
        //    }
        //    else
        //    {
        //        // need to prevent duplicates although we should allow for
        //        // results to be added after the countdown expires
        //    }
        //}

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
                m_raceState = RaceState.FinishCountdown;
                CountdownToRaceStage(RaceState.FinishCountdown, countDownMilliseconds);
            }
            else
            {
                m_races.Last().Finish();
                m_raceState = RaceState.Finished;
                m_milliSecondsUntilRaceFinish = 0;
                m_raceState = RaceState.Finished;
            }
        }

        #endregion public methods

        #region private methods

        private Task CountdownToRaceStage(RaceState state, long countDownDuration)
        {
            m_cancellationTokenSource = new CancellationTokenSource();

            return Task.Run(() =>
            {
                Stopwatch countDownStopwatch = new Stopwatch();
                countDownStopwatch.Start();

                while (countDownStopwatch.ElapsedMilliseconds < countDownDuration)
                {
                    if (m_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        if (state == RaceState.StartCountdown)
                        {
                            m_raceState = RaceState.Idle;
                            m_milliSecondsUntilRaceStart = -1;
                        }
                        else if (state == RaceState.FinishCountdown)
                        {
                            m_raceState = RaceState.InProgress;
                            m_milliSecondsUntilRaceFinish = -1;
                        }

                        m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }

                    // update timers
                    if (state == RaceState.StartCountdown)
                    {
                        m_milliSecondsUntilRaceStart = countDownDuration - countDownStopwatch.ElapsedMilliseconds;
                    }
                    else if (state == RaceState.FinishCountdown)
                    {
                        m_milliSecondsUntilRaceFinish = countDownDuration - countDownStopwatch.ElapsedMilliseconds;
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
            }, m_cancellationTokenSource.Token);
        }

        #endregion private methods
    }
}