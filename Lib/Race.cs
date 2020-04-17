using System;
using System.Collections.Generic;

namespace WebAppPrototype.Lib
{
    public class Race
    {
        private DateTime m_startTime;
        private DateTime m_endTime;
        private readonly int m_numberOfLaps;
        private List<int> m_finishedParticipants;
        private Dictionary<int, List<Lap>> m_participantsAndResults; // instead of storing results here, use DB and link with LapTimer's lap store

        public Race(int lapCount)
        {
            m_numberOfLaps = lapCount;
            m_finishedParticipants = new List<int>();
            m_participantsAndResults = new Dictionary<int, List<Lap>>();
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

        public void AddParticipant(int id)
        {
            m_participantsAndResults.TryAdd(id, new List<Lap>());
        }

        public void AddLapResult(int id, Lap lap)
        {
            m_participantsAndResults[id].Add(lap);
            if (m_participantsAndResults[id].Count >= m_numberOfLaps && !m_finishedParticipants.Contains(id))
            {
                m_finishedParticipants.Add(id);
            }
        }

        public bool AddResult(int id, List<Lap> laps)
        {
            return m_participantsAndResults.TryAdd(id, laps);
        }

        public Dictionary<int, List<Lap>> GetResults()
        {
            return m_participantsAndResults;
        }

        public bool HasAnyParticipantFinished()
        {
            return m_finishedParticipants.Count > 0;
        }

        public List<int> GetFinishedParticipants()
        {
            return m_finishedParticipants;
        }

        public bool HaveAllParticipantsFinished()
        {
            bool allCompleted = false;

            foreach (int id in m_participantsAndResults.Keys)
            {
                if (!m_finishedParticipants.Contains(id))
                {
                    allCompleted = false;
                    break;
                }
                allCompleted = true;
            }

            return allCompleted;
        }
    }
}