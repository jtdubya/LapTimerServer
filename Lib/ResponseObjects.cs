namespace LapTimerServer.Lib
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Json is all lowercase")]
    public class ResponseObjects
    {
        public string message { get; set; }

        public class Register : ResponseObjects
        {
            public int id { get; set; }
        }
    }
}