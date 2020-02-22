using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebAppPrototype.Lib
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
        /// Registers a lap timer by IP address
        /// </summary>
        /// <param name="lapTimerIpAddress">The IP address of the lap timer</param>
        /// <returns>ID of lap timer or -1 if a timer with the provided IP address alread exists</returns>
        public int RegisterLapTimer(IPAddress lapTimerIpAddress)
        {
            int currentId = m_nextId;
            m_nextId++;

            LapTimer lapTimer = new LapTimer(currentId);
            bool added = m_lapTimers.TryAdd(lapTimerIpAddress, lapTimer);

            if (!added)
            {
                currentId = -1;
            }

            return currentId;
        }

        public Dictionary<IPAddress, LapTimer> GetAllLapTimers()
        {
            return m_lapTimers;
        }
    }
}