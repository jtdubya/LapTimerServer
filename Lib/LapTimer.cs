using System;
using System.Collections.Generic;
using System.Linq;

namespace LapTimerServer.Lib
{
    public class LapTimer
    {
        private readonly int _id;
        private readonly List<Lap> _laps;

        public LapTimer(int id)
        {
            _id = id;
            _laps = new List<Lap>();
        }

        public int GetId()
        {
            return _id;
        }

        public List<Lap> GetAllLaps()
        {
            return _laps;
        }

        public Lap AddLap(TimeSpan timeSpan)
        {
            int nextLapNumber = _laps.Count() + 1;
            Lap newLap = new Lap(nextLapNumber, timeSpan);
            _laps.Add(newLap);
            return newLap;
        }

        public int GetLapCount()
        {
            return _laps.Count();
        }

        public TimeSpan GetTotalTime()
        {
            TimeSpan totalTime = new TimeSpan(0);

            if (_laps.Count() > 0)
            {
                for (int i = 0; i < _laps.Count(); i++)
                {
                    totalTime += _laps[i].Time;
                }
            }

            return totalTime;
        }

        public Lap GetFastestLap()
        {
            Lap fastestLap = new Lap(0, new TimeSpan(0));

            if (_laps.Count() > 0)
            {
                fastestLap = _laps[0];

                for (int i = 1; i < _laps.Count(); i++)
                {
                    if (_laps[i].Time < fastestLap.Time)
                    {
                        fastestLap = _laps[i];
                    }
                }
            }

            return fastestLap;
        }
    }
}