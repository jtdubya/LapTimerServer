using Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppPrototype.Lib
{
    public class LapTimer
    {
        private readonly int m_id;
        private int m_lapCount;
        private List<Lap> m_laps;

        public LapTimer(int id)
        {
            m_id = id;
            m_lapCount = 0;
            m_laps = new List<Lap>();
        }

        public void AddLap(TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public int GetLapCount()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetTotalTime()
        {
            throw new NotImplementedException();
        }

        public Lap GetFastestLap()
        {
            throw new NotImplementedException();
        }
    }
}