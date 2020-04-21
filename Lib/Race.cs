using System;
using System.Collections.Generic;

namespace LapTimerServer.Lib
{
    public class Race
    {
        private DateTime _startTime;
        private DateTime _endTime;
        private readonly int _numberOfLaps;
        private readonly List<int> _finishedParticipants;
        private readonly Dictionary<int, List<Lap>> _participantsAndResults; // instead of storing results here, use DB and link with LapTimer's lap store

        public Race(int lapCount)
        {
            _numberOfLaps = lapCount;
            _finishedParticipants = new List<int>();
            _participantsAndResults = new Dictionary<int, List<Lap>>();
        }

        public DateTime Start()
        {
            _startTime = DateTime.Now;
            return _startTime;
        }

        public DateTime Finish()
        {
            _endTime = DateTime.Now;
            return _endTime;
        }

        public TimeSpan GetDuration()
        {
            // {1/1/0001 12:00:00 AM}
            return _endTime - _startTime;
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