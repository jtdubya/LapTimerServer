using System.Collections.Generic;
using System.Net;

namespace WebAppPrototype.Lib
{
    public class LapTimerManager
    {
        private int m_nextId;
        private Dictionary<IPAddress, LapTimer> m_lapTimers;
        private LapTimer existingtimer;

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
            int returnId = -1;
            bool alreadyRegistered = m_lapTimers.TryGetValue(lapTimerIpAddress, out LapTimer existingtimer);

            if (alreadyRegistered)
            {
                returnId = existingtimer.GetId();
            }
            else
            {
                LapTimer lapTimer = new LapTimer(m_nextId);
                bool added = m_lapTimers.TryAdd(lapTimerIpAddress, lapTimer);

                if (added)
                {
                    returnId = m_nextId;
                    m_nextId++;
                }
            }

            return returnId;
        }

        public Dictionary<IPAddress, LapTimer> GetAllLapTimers()
        {
            return m_lapTimers;
        }
    }
}