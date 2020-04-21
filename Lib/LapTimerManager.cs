using System;
using System.Collections.Generic;
using System.Net;

namespace LapTimerServer.Lib
{
    public class LapTimerManager
    {
        private int _nextId;
        private readonly Dictionary<IPAddress, LapTimer> _lapTimers;

        public LapTimerManager()
        {
            _nextId = 1;
            _lapTimers = new Dictionary<IPAddress, LapTimer>();
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
                LapTimer lapTimer = new LapTimer(_nextId);
                bool added = _lapTimers.TryAdd(lapTimerIpAddress, lapTimer);

                if (added)
                {
                    returnId = _nextId;
                    _nextId++;
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
            _lapTimers.TryGetValue(lapTimerIpAddress, out LapTimer existingtimer);
            return existingtimer;
        }

        public Dictionary<IPAddress, LapTimer> GetAllLapTimers()
        {
            return _lapTimers;
        }

        public Lap AddLapResult(IPAddress iPAddress, TimeSpan lapTime)
        {
            return _lapTimers[iPAddress].AddLap(lapTime);
        }
    }
}