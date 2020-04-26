using System;
using System.Collections.Generic;
using LapTimerServer.Lib;

namespace LapTimerServer.JsonObjects
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Json is all lowercase")]
    public class ResponseObject

    {
        public string responseMessage { get; set; }

        public class Participants : ResponseObject
        {
            public int maxParticipants { get; set; }
        }

        public class State : ResponseObject
        {
            public RaceState state { get; set; }
            public string stateName { get; set; }
        }

        public class Register : ResponseObject
        {
            public int id { get; set; }
        }

        public class TimeUntilStart : ResponseObject
        {
            public int numberOfLaps { get; set; }
            public long millisecondsUntilStart { get; set; }
        }

        public class Start : ResponseObject
        {
            public long raceStartCountdownDuration { get; set; }
            public long millisSecondsUntilRaceStart { get; set; }
        }

        public class LapResult
        {
            public int timerID { get; set; }
            public List<string> laps { get; set; }
        }

        public class Race : ResponseObject
        {
            public string raceState { get; set; }
            public int numberOfLaps { get; set; }
            public DateTime startTime { get; set; }
            public DateTime finishTime { get; set; }
            public TimeSpan duration { get; set; }

            public List<int> finishOrder { get; set; }

            public List<LapResult> lapResults { get; set; }
        }

        public class RaceResultByID : ResponseObject // this is meant to be a much smaller response than the Race response object
        {
            public int id { get; set; }
            public int place { get; set; }
            public string overallTime { get; set; }
            public double overallTimeMilliseconds { get; set; } // milliseconds are easier to parse by software clients
            public string fastestLap { get; set; }
            public double fastestLapMilliseconds { get; set; }
            public int fastestLapNumber { get; set; }
        }
    }
}