using System;
using System.Collections.Generic;
using LapTimerServer.Lib;
using Xunit;

namespace LapTimerServer.LibUnitTests
{
    public class RaceUnitTests
    {
        private Race m_race = new Race(10);

        [Fact]
        public void Start()
        {
            DateTime before = DateTime.Now;
            DateTime start = m_race.Start();

            Assert.True(before < start);
            Assert.True(start < DateTime.Now);
        }

        [Fact]
        public void Finish()
        {
            DateTime before = DateTime.Now;
            DateTime start = m_race.Finish();

            Assert.True(before < start);
            Assert.True(start < DateTime.Now);
        }

        [Fact]
        public void GetDuration()
        {
            DateTime start = m_race.Start();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.175));
            DateTime finish = m_race.Finish();

            Assert.Equal(finish - start, m_race.GetDuration());
        }

        [Fact]
        public void GetNumberOfLaps()
        {
            Assert.Equal(10, m_race.GetNumberOfLaps());
        }

        [Fact]
        public void AddResultsAndGetResults()
        {
            int days = 0, hours = 0, minutes = 2, seconds = 0, millis = 0;
            TimeSpan car1Lap1 = new TimeSpan(days, hours, minutes, seconds, millis + 10);
            TimeSpan car1Lap2 = new TimeSpan(days, hours, minutes, seconds + 1, millis);
            TimeSpan car2Lap1 = new TimeSpan(days, hours, minutes, seconds + 2, millis);
            TimeSpan car2Lap2 = new TimeSpan(days, hours, minutes, seconds, millis + 250);

            List<Lap> car1Laps = new List<Lap> { new Lap(1, car1Lap1), new Lap(2, car1Lap2) };
            List<Lap> car2Laps = new List<Lap> { new Lap(1, car2Lap1), new Lap(2, car2Lap2) };

            m_race.AddResult(1, car1Laps);
            m_race.AddResult(2, car2Laps);

            var results = m_race.GetResults();

            Assert.True(results.ContainsKey(1));
            Assert.True(results.ContainsKey(2));

            results.TryGetValue(1, out var result1);
            results.TryGetValue(2, out var result2);

            Assert.Equal(1, result1[0].Number);
            Assert.Equal(car1Lap1, result1[0].Time);
            Assert.Equal(2, result1[1].Number);
            Assert.Equal(car1Lap2, result1[1].Time);

            Assert.Equal(1, result2[0].Number);
            Assert.Equal(car2Lap1, result2[0].Time);
            Assert.Equal(2, result2[1].Number);
            Assert.Equal(car2Lap2, result2[1].Time);
        }
    }
}