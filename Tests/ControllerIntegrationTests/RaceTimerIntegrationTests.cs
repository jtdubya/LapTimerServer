using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace LapTimerServer.Tests.ControllerIntegrationTests
{
    public class RaceTimerIntegrationTests : IDisposable
    {
        private readonly string prefix = "api/v1/RaceTimer";
        private readonly TestServer _testServer;
        private readonly HttpClient _httpClient;

        public RaceTimerIntegrationTests()
        {
            _testServer = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _httpClient = _testServer.CreateClient();
        }

        public void Dispose()
        {
            _testServer.Dispose();
            _httpClient.Dispose();
        }

        [Fact]
        public async Task Register()
        {
            var response = await _httpClient.GetAsync(prefix + "/Register/1.1.1.1");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("1", responseString);
        }
    }
}