using System;

namespace LapTimerServer.Lib
{
    public struct Lap
    {
        public Lap(int lapNumber, TimeSpan lapTime)
        {
            Number = lapNumber;
            Time = lapTime;
        }

        public int Number { get; set; }

        public TimeSpan Time { get; }

        public override string ToString() => $"{Number}: {Time}";
    }
}