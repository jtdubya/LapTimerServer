using System;

namespace LapTimerServer.Lib
{
    public readonly struct Lap
    {
        public Lap(int lapNumber, TimeSpan lapTime)
        {
            Number = lapNumber;
            Time = lapTime;
        }

        public int Number { get; }

        public TimeSpan Time { get; }

        public readonly override string ToString() => $"{Number}: {Time}";
    }
}