namespace LapTimerServer.JsonObjects
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Json is all lowercase")]
    public class RequestObject
    {
        public string ipAddress { get; set; }

        public class LapResult : RequestObject
        {
            public string lapTime { get; set; }
        }

        public class LapResultMilliseconds : RequestObject
        {
            public double lapTime { get; set; }
        }
    }
}