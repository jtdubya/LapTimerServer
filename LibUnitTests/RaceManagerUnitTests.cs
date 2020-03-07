using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebAppPrototype.Lib;
using Xunit;

namespace WebAppPrototype.LibUnitTests
{
    public class RaceManagerUnitTests
    {
        private RaceManager m_raceManager;

        public RaceManagerUnitTests()
        {
            m_raceManager = new RaceManager();
        }

        [Theory]
        [InlineData(50)]
        [InlineData(1)]
        public void GetSetMaxParticipants(int maxParticipants)
        {
            m_raceManager.SetMaxParticipants(maxParticipants);
            Assert.Equal(maxParticipants, m_raceManager.GetMaxParticipants());
        }

        [Fact]
        public void Register_IdleStateWithDuplicates_ReturnsRegisteredID()
        {
            int first = m_raceManager.Register("10.0.1.1");
            int second = m_raceManager.Register("10.0.1.2");
            int secondDuplicate = m_raceManager.Register("10.0.1.2");
            int firstDuplicate = m_raceManager.Register("10.0.1.1");
            int firstTriplicate = m_raceManager.Register("10.0.1.1");

            Assert.Equal(1, first);
            Assert.Equal(1, firstDuplicate);
            Assert.Equal(1, firstTriplicate);
            Assert.Equal(2, second);
            Assert.Equal(2, secondDuplicate);
            Assert.Equal(2, m_raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleState()
        {
            m_raceManager.SetMaxParticipants(3);
            int first = m_raceManager.Register("10.0.1.1");
            int second = m_raceManager.Register("10.0.1.2");
            int third = m_raceManager.Register("10.0.1.3");

            Assert.Equal(1, first);
            Assert.Equal(2, second);
            Assert.Equal(3, third);
            Assert.Equal(3, m_raceManager.GetParticipants().Count);
        }

        [Fact]
        public void Register_IdleStateWithMoreThanMax_ThrowsException()
        {
            m_raceManager.SetMaxParticipants(1);
            int first = m_raceManager.Register("10.0.1.1");

            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("10.0.1.2"));

            Assert.Contains("Registration closed. Max participants reached.", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithBadIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("l;sakdjfflsakdjf"));
            Assert.Contains("Could not parse IP address", exception.Message);
        }

        [Fact]
        public void Register_IdleStateWithNullIPAddress_ThrowsException()
        {
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register(null));
            Assert.Contains("Must provide device IP address to register", exception.Message);
        }

        [Fact]
        public void Register_NonIdleState_ThrowException()
        {
            m_raceManager.StartRace();
            Exception exception = Assert.Throws<InvalidOperationException>(() => m_raceManager.Register("1.1.1.1"));
            Assert.Contains("Registration closed. Wait until next registration period.", exception.Message);
        }
    }
}