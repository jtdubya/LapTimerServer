using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using LapTimerServer.Lib;
using System.ComponentModel.Design.Serialization;
using System.Net;

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
        public async Task GetSetMaxParticipants()
        {
            var response = await _httpClient.GetAsync(prefix + "/GetMaxparticipants");
            response.EnsureSuccessStatusCode();
            var getMaxResponse = JsonSerializer.Deserialize<ResponseObject.Participants>(
                await response.Content.ReadAsStringAsync()
                );
            Assert.IsType<int>(getMaxResponse.maxParticipants);

            int newMax = getMaxResponse.maxParticipants + 10;

            var setResponse = await _httpClient.GetAsync(prefix + "/SetMaxParticipants/" + newMax);
            response.EnsureSuccessStatusCode();
            var setMaxResponse = JsonSerializer.Deserialize<ResponseObject.Participants>(
                await setResponse.Content.ReadAsStringAsync()
                );
            Assert.Equal(newMax, setMaxResponse.maxParticipants);

            var finalResponse = await _httpClient.GetAsync(prefix + "/GetMaxparticipants");
            finalResponse.EnsureSuccessStatusCode();
            var getMaxResponse2 = JsonSerializer.Deserialize<ResponseObject.Participants>(
                await finalResponse.Content.ReadAsStringAsync()
                );
            Assert.Equal(newMax, getMaxResponse2.maxParticipants);
        }

        [Fact]
        public async Task GetSetRaceStartCountdownDuration()
        {
            var getResponse = await _httpClient.GetAsync(prefix + "/GetRaceStartCountdownDuration");
            getResponse.EnsureSuccessStatusCode();
            var getRaceStartCountdownDuration = JsonSerializer.Deserialize<ResponseObject.Start>(
                await getResponse.Content.ReadAsStringAsync()
                );
            Assert.IsType<long>(getRaceStartCountdownDuration.raceStartCountdownDuration);

            long newCountdownDuration = getRaceStartCountdownDuration.raceStartCountdownDuration + 1000;

            var setResponse = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + newCountdownDuration);
            setResponse.EnsureSuccessStatusCode();
            var setDurationResponse = JsonSerializer.Deserialize<ResponseObject.Start>(
                await setResponse.Content.ReadAsStringAsync()
                );
            Assert.Equal(newCountdownDuration, setDurationResponse.raceStartCountdownDuration);

            var getResponse2 = await _httpClient.GetAsync(prefix + "/GetRaceStartCountdownDuration");
            getResponse2.EnsureSuccessStatusCode();
            var getRaceStartCountdownDuration2 = JsonSerializer.Deserialize<ResponseObject.Start>(
                await getResponse2.Content.ReadAsStringAsync()
                );
            Assert.Equal(newCountdownDuration, getRaceStartCountdownDuration2.raceStartCountdownDuration);
        }

        [Fact]
        public async Task Register_Success()
        {
            var response = await _httpClient.GetAsync(prefix + "/Register/1.1.1.1");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(responseString);
            Assert.Equal(1, registerResponse.id);
            Assert.Equal("success", registerResponse.message);
        }

        [Fact]
        public async Task Register_MaxParticipantsReached()
        {
            var setResponse = await _httpClient.GetAsync(prefix + "/SetMaxParticipants/" + 1);
            setResponse.EnsureSuccessStatusCode();

            var response = await _httpClient.GetAsync(prefix + "/Register/1.1.1.1");
            response.EnsureSuccessStatusCode();

            var response2 = await _httpClient.GetAsync(prefix + "/Register/2.2.2.2");
            response.EnsureSuccessStatusCode();

            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(
                await response2.Content.ReadAsStringAsync()
                );

            Assert.Equal(-1, registerResponse.id);
            Assert.Contains("Registration closed. Max participants reached.", registerResponse.message);
        }

        [Fact]
        public async Task Register_RegistrationNotOpen()
        {
            var startResponse = await _httpClient.GetAsync(prefix + "/StartRace");
            startResponse.EnsureSuccessStatusCode();
            Thread.Sleep(5);

            var response = await _httpClient.GetAsync(prefix + "/Register/1.1.1.1");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(responseString);
            Assert.Equal(-1, registerResponse.id);
            Assert.Contains("Registration closed. Wait until next registration period.", registerResponse.message);
        }

        [Fact]
        public async Task Register_BadParameter()
        {
            var response = await _httpClient.GetAsync(prefix + "/Register/asdfsefe");
            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(-1, registerResponse.id);
            Assert.Contains("Could not parse IP address", registerResponse.message);
        }

        [Fact]
        public async Task StartRace()
        {
            int countDownDuration = 10000;
            var setResponse = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + countDownDuration);
            setResponse.EnsureSuccessStatusCode();
            var response = await _httpClient.GetAsync(prefix + "/StartRace");
            response.EnsureSuccessStatusCode();
            var startResponse = JsonSerializer.Deserialize<ResponseObject.Start>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("success", startResponse.message);
            Assert.Equal(countDownDuration, startResponse.raceStartCountdownDuration);
            Assert.InRange(startResponse.millisSecondsUntilRaceStart, 0, countDownDuration);
        }
    }
}