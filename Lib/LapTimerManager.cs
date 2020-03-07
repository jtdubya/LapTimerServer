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
        /// Registers a lap timer by IP address or returns id of timer if it is already registered
        /// </summary>
        public int RegisterLapTimer(IPAddress lapTimerIpAddress)
        {
            int returnId = GetLapTimerByIPAddress(lapTimerIpAddress);

            if (returnId == -1)
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

        /// <summary>
        /// Gets Lap Timer by IP Address if it is registered of -1 if not registered
        /// </summary>
        public int GetLapTimerByIPAddress(IPAddress lapTimerIpAddress)
        {
            int idToReturn = -1;
            bool alreadyRegistered = m_lapTimers.TryGetValue(lapTimerIpAddress, out LapTimer existingtimer);

            if (alreadyRegistered)
            {
                idToReturn = existingtimer.GetId();
            }

            return idToReturn;
        }

        public Dictionary<IPAddress, LapTimer> GetAllLapTimers()
        {
            return m_lapTimers;
        }
    }
}