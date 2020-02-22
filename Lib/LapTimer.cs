using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppPrototype.Lib
{
    public class LapTimer
    {
        private readonly int m_id;
        private List<Lap> m_laps;

        public LapTimer(int id)
        {
            m_id = id;
            m_laps = new List<Lap>();
        }

        public int GetId()
        {
            return m_id;
        }

        public void AddLap(TimeSpan timeSpan)
        {
            int nextLapNumber = m_laps.Count() + 1;
            m_laps.Add(new Lap(nextLapNumber, timeSpan));
        }

        public int GetLapCount()
        {
            return m_laps.Count();
        }

        public TimeSpan GetTotalTime()
        {
            TimeSpan totalTime = new TimeSpan(0);

            if (m_laps.Count() > 0)
            {
                for (int i = 0; i < m_laps.Count(); i++)
                {
                    totalTime += m_laps[i].GetLapTime();
                }
            }

            return totalTime;
        }

        public Lap GetFastestLap()
        {
            Lap fastestLap = new Lap(0, new TimeSpan(0));

            if (m_laps.Count() > 0)
            {
                fastestLap = m_laps[0];

                for (int i = 1; i < m_laps.Count(); i++)
                {
                    if (m_laps[i].GetLapTime() < fastestLap.GetLapTime())
                    {
                        fastestLap = m_laps[i];
                    }
                }
            }

            return fastestLap;
        }
    }
}