using System;
using System.Collections.Generic;

namespace WebAppPrototype.Lib
{
    public class Race
    {
        private DateTime m_startTime;
        private DateTime m_endTime;
        private int m_numberOfLaps;
        private Dictionary<int, List<Lap>> m_results;

        public Race(int lapCount)
        {
            m_numberOfLaps = lapCount;
            m_results = new Dictionary<int, List<Lap>>();
        }

        public DateTime Start()
        {
            m_startTime = DateTime.Now;
            return m_startTime;
        }

        public DateTime Finish()
        {
            m_endTime = DateTime.Now;
            return m_endTime;
        }

        public TimeSpan GetDuration()
        {
            // {1/1/0001 12:00:00 AM}
            return m_endTime - m_startTime;
        }

        public int GetNumberOfLaps()
        {
            return m_numberOfLaps;
        }

        public bool AddResult(int id, List<Lap> laps)
        {
            return m_results.TryAdd(id, laps);
        }

        public Dictionary<int, List<Lap>> GetResults()
        {
            return m_results;
        }
    }
}