using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LapTimerServer.JsonObjects
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Json is all lowercase")]
    public class RequestObject
    {
        public class LapResult
        {
            public string ipAddress { get; set; }
            public string lapTime { get; set; }
        }
    }
}