using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace WebAppPrototype.Lib
{
    public enum RaceState
    {
        Idle,
        Start,
        InProgress,
        Finished
    }

    public class RaceManager
    {
        private int m_maxParticipants;
        private RaceState m_raceState;
        private readonly LapTimerManager m_lapTimerManager;
        private readonly LapTimerMessageHandler m_lapTimerMessageHandler;

        public RaceManager()
        {
            m_maxParticipants = 2;
            m_raceState = RaceState.Idle;
            m_lapTimerManager = new LapTimerManager();
            m_lapTimerMessageHandler = new LapTimerMessageHandler();
        }

        public RaceState GetRaceState()
        {
            return m_raceState;
        }

        public int GetMaxParticipants()
        {
            return m_maxParticipants;
        }

        public void SetMaxParticipants(int newMax)
        {
            m_maxParticipants = newMax;
        }

        public int Register(string ipAddressString)
        {
            int id = -1; // only set to positive number if registration is successfull
            string errorMessage = "Registration closed. ";

            if (m_raceState == RaceState.Idle)
            {
                try
                {
                    IPAddress ipAddress;
                    ipAddress = IPAddress.Parse(ipAddressString);
                    id = m_lapTimerManager.GetLapTimerByIPAddress(ipAddress);

                    if (id == -1) // not registered
                    {
                        if (m_lapTimerManager.GetAllLapTimers().Count >= m_maxParticipants)
                        {
                            errorMessage += "Max participants reached.";
                        }
                        else
                        {
                            id = m_lapTimerManager.RegisterLapTimer(ipAddress);
                        }
                    }
                }
                catch (FormatException)
                {
                    errorMessage = "Could not parse IP address";
                }
                catch (ArgumentNullException)
                {
                    errorMessage = "Must provide device IP address to register";
                }
            }
            else
            {
                errorMessage += "Wait until next registration period.";
            }

            if (id == -1)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return id;
        }

        public Dictionary<IPAddress, LapTimer> GetParticipants()
        {
            return m_lapTimerManager.GetAllLapTimers();
        }

        public void StartRace()
        {
            m_raceState = RaceState.Start;
        }

        public void FinishRace()
        {
            m_raceState = RaceState.Finished;
        }
    }
}