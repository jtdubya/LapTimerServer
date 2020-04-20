using System;
using System.Collections.Generic;
using System.Net;

namespace LapTimerServer.Lib
{
    public class LapTimerManager
    {
        private int m_nextId;
        private Dictionary<IPAddress, LapTimer> m_lapTimers;

        public LapTimerManager()
        {
            m_nextId = 1;
            m_lapTimers = new Dictionary<IPAddress, LapTimer>();
        }

        /// <summary>
        /// Registers a lap timer by IP address or returns id of timer if it is already registered
        /// </summary>
        public int RegisterLapTimer(IPAddress lapTimerIpAddress)
        {
            int returnId = -1;
            LapTimer existingTimer = GetLapTimerByIPAddress(lapTimerIpAddress);

            if (existingTimer == null)
            {
                LapTimer lapTimer = new LapTimer(m_nextId);
                bool added = m_lapTimers.TryAdd(lapTimerIpAddress, lapTimer);

                if (added)
                {
                    returnId = m_nextId;
                    m_nextId++;
                }
            }
            else
            {
                returnId = existingTimer.GetId();
            }

            return returnId;
        }

        public LapTimer GetLapTimerByIPAddress(IPAddress lapTimerIpAddress)
        {
            m_lapTimers.TryGetValue(lapTimerIpAddress, out LapTimer existingtimer);
            return existingtimer;
        }

        public Dictionary<IPAddress, LapTimer> GetAllLapTimers()
        {
            return m_lapTimers;
        }

        public Lap AddLapResult(IPAddress iPAddress, TimeSpan lapTime)
        {
            return m_lapTimers[iPAddress].AddLap(lapTime);
        }
    }
}