namespace LapTimerServer.Lib
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Json is all lowercase")]
    public class ResponseObject

    {
        public string message { get; set; }

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

        public class Start : ResponseObject
        {
            public long raceStartCountdownDuration { get; set; }
            public long millisSecondsUntilRaceStart { get; set; }
        }
    }
}