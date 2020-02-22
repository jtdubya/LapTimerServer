using System;

namespace WebAppPrototype.Lib
{
    public class Lap
    {
        private readonly int m_lapNumber;
        private readonly TimeSpan m_lapTime;

        public Lap(int lapNumber, TimeSpan lapTime)
        {
            m_lapNumber = lapNumber;
            m_lapTime = lapTime;
        }

        public int GetLapNumber()
        {
            return m_lapNumber;
        }

        public TimeSpan GetLapTime()
        {
            return m_lapTime;
        }
    }
}