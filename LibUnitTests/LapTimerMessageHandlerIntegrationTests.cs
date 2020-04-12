using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net;
using Xunit;
using WebAppPrototype.Lib;

namespace WebAppPrototype.LibUnitTests
{
    public class LapTimerMessageHandlerIntegrationTests : IDisposable
    {
        private readonly LapTimerMessageHandler m_LapTimerMessageHandler;

        public LapTimerMessageHandlerIntegrationTests()
        {
            m_LapTimerMessageHandler = new LapTimerMessageHandler();
        }

        public void Dispose()
        {
            m_LapTimerMessageHandler.Dispose();
        }

        //[Fact]
        //public void SendRaceStartNotification()
        //{
        //    string ipAddress = "127.0.0.1";
        //    var message = m_LapTimerMessageHandler.SendRaceStartNotification(IPAddress.Parse(ipAddress), 5000);
        //    message.Wait();
        //    // assert ...
        //}
    }
}