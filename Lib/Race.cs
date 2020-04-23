using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LapTimerServer.Lib
{
    public class Race
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private readonly Stopwatch _stopwatch;
        private readonly int _numberOfLaps;
        private readonly List<int> _finishedParticipants;
        private readonly Dictionary<int, List<Lap>> _participantsAndResults; // instead of storing results here, use DB and link with LapTimer's lap store

        public Race(int lapCount)
        {
            _stopwatch = new Stopwatch();
            _numberOfLaps = lapCount;
            _finishedParticipants = new List<int>();
            _participantsAndResults = new Dictionary<int, List<Lap>>();
        }

        public DateTime Start()
        {
            _stopwatch.Reset();
            _startTime = DateTime.Now;
            _stopwatch.Start();
            return _startTime;
        }

        public DateTime GetStartTime()
        {
            return _startTime;
        }

        public DateTime Finish()
        {
            _stopwatch.Stop();
            _endTime = DateTime.Now;
            return _endTime;
        }

        public DateTime GetFinishTime()
        {
            return _endTime;
        }

        public TimeSpan GetDuration()
        {
            return _stopwatch.Elapsed;
        }

        public int GetNumberOfLaps()
        {
            return _numberOfLaps;
        }

        public void AddParticipant(int id)
        {
            _participantsAndResults.TryAdd(id, new List<Lap>());
        }

        public void AddLapResult(int id, Lap lap)
        {
            _participantsAndResults[id].Add(lap);
            if (_participantsAndResults[id].Count >= _numberOfLaps && !_finishedParticipants.Contains(id))
            {
                _finishedParticipants.Add(id);
            }
        }

        public bool AddResult(int id, List<Lap> laps)
        {
            return _participantsAndResults.TryAdd(id, laps);
        }

        public Dictionary<int, List<Lap>> GetResults()
        {
            return _participantsAndResults;
        }

        public bool HasAnyParticipantFinished()
        {
            return _finishedParticipants.Count > 0;
        }

        public List<int> GetFinishedParticipants()
        {
            return _finishedParticipants;
        }

        public bool HaveAllParticipantsFinished()
        {
            bool allCompleted = false;

            foreach (int id in _participantsAndResults.Keys)
            {
                if (!_finishedParticipants.Contains(id))
                {
                    allCompleted = false;
                    break;
                }
                allCompleted = true;
            }

            return allCompleted;
        }
    }
}