using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using LapTimerServer.Lib;
using System.Net;
using System.Text;
using LapTimerServer.JsonObjects;

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
            Assert.Equal("success", registerResponse.responseMessage);
        }

        [Fact]
        public async Task Register_MaxParticipantsReached()
        {
            var setResponse = await _httpClient.GetAsync(prefix + "/SetMaxParticipants/" + 1);
            setResponse.EnsureSuccessStatusCode();

            var response = await _httpClient.GetAsync(prefix + "/Register/1.1.1.1");
            response.EnsureSuccessStatusCode();

            var response2 = await _httpClient.GetAsync(prefix + "/Register/2.2.2.2");
            response2.EnsureSuccessStatusCode();

            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(
                await response2.Content.ReadAsStringAsync()
                );

            Assert.Equal(-2, registerResponse.id);
            Assert.Contains("Registration closed. Max participants reached.", registerResponse.responseMessage);
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
            Assert.Contains("Registration closed. Wait until next registration period.", registerResponse.responseMessage);
        }

        [Fact]
        public async Task Register_BadParameter()
        {
            var response = await _httpClient.GetAsync(prefix + "/Register/asdfsefe");
            var registerResponse = JsonSerializer.Deserialize<ResponseObject.Register>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(-1, registerResponse.id);
            Assert.Contains("Could not parse IP address", registerResponse.responseMessage);
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

            Assert.Equal("success", startResponse.responseMessage);
            Assert.Equal(countDownDuration, startResponse.raceStartCountdownDuration);
            Assert.InRange(startResponse.millisSecondsUntilRaceStart, 0, countDownDuration);
        }

        [Fact]
        public async Task GetRaceState_InitialStateIsRegistration()
        {
            var response = await _httpClient.GetAsync(prefix + "/GetRaceState");
            response.EnsureSuccessStatusCode();

            var stateResponse = JsonSerializer.Deserialize<ResponseObject.State>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(RaceState.Registration, stateResponse.state);
            Assert.Equal("Registration", stateResponse.stateName);
        }

        [Fact]
        public async Task AddLapResult_Success()
        {
            string ipAddress = "10.1.1.1";

            var registerResponse = await _httpClient.GetAsync(prefix + "/Register/" + ipAddress);
            registerResponse.EnsureSuccessStatusCode();

            var lapResult = new RequestObject.LapResult
            {
                ipAddress = ipAddress,
                lapTime = "0:0:1:12"
            };
            var jsonString = JsonSerializer.Serialize(lapResult);
            var lapContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(prefix + "/AddLapResultAsString", lapContent);
            var responseObject = JsonSerializer.Deserialize<ResponseObject>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("success", responseObject.responseMessage);

            var lapResultMillis = new RequestObject.LapResultMilliseconds
            {
                ipAddress = ipAddress,
                lapTime = 123456
            };
            var jsonString2 = JsonSerializer.Serialize(lapResultMillis);
            var lapContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");

            var response2 = await _httpClient.PostAsync(prefix + "/AddLapResultInMilliseconds", lapContent2);
            var responseObject2 = JsonSerializer.Deserialize<ResponseObject>(
                await response2.Content.ReadAsStringAsync());
            Assert.Equal("success", responseObject2.responseMessage);
        }

        [Fact]
        public async Task AddLapResult_NotRegisteredError()
        {
            var lapResult = new RequestObject.LapResult
            {
                ipAddress = "10.1.1.1",
                lapTime = "0:0:1:12"
            };
            var jsonString = JsonSerializer.Serialize(lapResult);
            var lapContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(prefix + "/AddLapResultAsString", lapContent);
            response.EnsureSuccessStatusCode();
            var responseObject = JsonSerializer.Deserialize<ResponseObject>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("The given ip address '10.1.1.1' is not registered.", responseObject.responseMessage);
        }

        [Fact]
        public async Task GetTimeUntilRaceState_NotInStartCoundownState()
        {
            var response = await _httpClient.GetAsync(prefix + "/GetTimeUntilRaceStart");
            response.EnsureSuccessStatusCode();
            var responseObject = JsonSerializer.Deserialize<ResponseObject.TimeUntilStart>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("success", responseObject.responseMessage);
            Assert.Equal(-1, responseObject.millisecondsUntilStart);
        }

        [Fact]
        public async Task GetTimeUntilRaceState_InStartCoundownState()
        {
            int countDownDuration = 1000;
            var response = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + countDownDuration);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/StartRace");
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/GetTimeUntilRaceStart");
            response.EnsureSuccessStatusCode();
            var responseObject = JsonSerializer.Deserialize<ResponseObject.TimeUntilStart>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(new RaceManager().NumberOfLaps, responseObject.numberOfLaps);
            Assert.Equal("success", responseObject.responseMessage);
            Assert.InRange(responseObject.millisecondsUntilStart, 1, countDownDuration);
        }

        [Fact]
        public async Task GetLastRaceResultById_RaceIsNotFinished()
        {
            int id = 1;
            var response = await _httpClient.GetAsync(prefix + "/GetLastRaceResultById/" + id);
            response.EnsureSuccessStatusCode();
            var raceResult = JsonSerializer.Deserialize<ResponseObject.RaceResultByID>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(id, raceResult.id);
            Assert.Equal("Race is not finished.", raceResult.responseMessage);
        }

        [Fact]
        public async Task GetLastRaceResultById_IdNotFound()
        {
            string ip = "1.1.1.1";
            var response = await _httpClient.GetAsync(prefix + "/Register/" + ip);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + 0);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/StartRace");
            response.EnsureSuccessStatusCode();

            // this test is dependent on the default number of laps being 10 (haven't implement a way to change it yet)
            for (int lap = 1; lap <= 10; lap++)
            {
                var lapResult = new RequestObject.LapResult
                {
                    ipAddress = ip,
                    lapTime = "0:0:1:0"
                };

                var jsonString = JsonSerializer.Serialize(lapResult);
                var lapContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync(prefix + "/AddLapResultAsString", lapContent);
            }

            response = await _httpClient.GetAsync(prefix + "/GetLastRaceResultById/" + 4356);
            response.EnsureSuccessStatusCode();
            var raceResult = JsonSerializer.Deserialize<ResponseObject.RaceResultByID>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal(4356, raceResult.id);
            Assert.Equal("ID [4356] was not found.", raceResult.responseMessage);
        }

        [Fact]
        [Trait("Category", "Big Test")]
        public async Task GetLastRaceResultById_MultipleIDs()
        {
            string ip1 = "1.1.1.1";
            string ip2 = "2.2.2.2";
            string ip3 = "3.3.3.3";
            var response = await _httpClient.GetAsync(prefix + "/SetMaxParticipants/" + 3);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/Register/" + ip1);
            response.EnsureSuccessStatusCode();
            response = await _httpClient.GetAsync(prefix + "/Register/" + ip2);
            response.EnsureSuccessStatusCode();
            var ip2response = JsonSerializer.Deserialize<ResponseObject.Register>(
                await response.Content.ReadAsStringAsync());
            int id2 = ip2response.id;

            response = await _httpClient.GetAsync(prefix + "/Register/" + ip3);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + 0);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/StartRace");
            response.EnsureSuccessStatusCode();

            // this test is dependent on the default number of laps being 10 (haven't implement a way to change it yet)
            for (int lap = 1; lap <= 10; lap++)
            {
                var lapResult = new RequestObject.LapResultMilliseconds
                {
                    lapTime = 61010
                };

                if (lap == 3)
                {
                    lapResult.lapTime = 59015; // fastest lap
                }

                foreach (string ipAddress in new List<string> { ip3, ip2, ip1 })
                {
                    lapResult.ipAddress = ipAddress;
                    var jsonString = JsonSerializer.Serialize(lapResult);
                    var lapContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync(prefix + "/AddLapResultInMilliseconds", lapContent);
                }
            }

            response = await _httpClient.GetAsync(prefix + "/GetLastRaceResultById/" + id2);
            response.EnsureSuccessStatusCode();
            var raceResult = JsonSerializer.Deserialize<ResponseObject.RaceResultByID>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("success", raceResult.responseMessage);
            Assert.Equal(id2, raceResult.id);
            Assert.Equal(2, raceResult.place);
            Assert.Equal("00:10:08.1050000", raceResult.overallTime);
            Assert.Equal(608105, raceResult.overallTimeMilliseconds);
            Assert.Equal("00:00:59.0150000", raceResult.fastestLap);
            Assert.Equal(59015, raceResult.fastestLapMilliseconds);
            Assert.Equal(3, raceResult.fastestLapNumber);
        }

        [Fact]
        public async Task GetCurrentRaceResults_NoRaces()
        {
            var response = await _httpClient.GetAsync(prefix + "/GetCurrentRaceResults");
            response.EnsureSuccessStatusCode();
            var responseObject = JsonSerializer.Deserialize<ResponseObject>(
                await response.Content.ReadAsStringAsync());

            Assert.Equal("No races available.", responseObject.responseMessage);
        }

        [Fact]
        [Trait("Category", "Big Test")]
        public async Task GetCurrentRaceResults_MultipleRaces()
        {
            string ip1 = "1.1.1.1";
            string ip2 = "2.2.2.2";
            var response = await _httpClient.GetAsync(prefix + "/SetMaxParticipants/" + 2);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/Register/" + ip1);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/Register/" + ip2);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/SetRaceStartCountdownDuration/" + 0);
            response.EnsureSuccessStatusCode();

            response = await _httpClient.GetAsync(prefix + "/StartRace");
            response.EnsureSuccessStatusCode();

            // this test is dependent on the default number of laps being 10 (haven't implement a way to change it yet)
            for (int lap = 1; lap <= 10; lap++)
            {
                var lapResult = new RequestObject.LapResult
                {
                    lapTime = "0:0:1:" + lap
                };

                foreach (string ipAddress in new List<string> { ip2, ip1 })
                {
                    lapResult.ipAddress = ipAddress;
                    var jsonString = JsonSerializer.Serialize(lapResult);
                    var lapContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    await _httpClient.PostAsync(prefix + "/AddLapResultAsString", lapContent);
                }
            }

            response = await _httpClient.GetAsync(prefix + "/GetCurrentRaceResults");
            response.EnsureSuccessStatusCode();
            var stringContent = await response.Content.ReadAsStringAsync();
            var raceResults = JsonSerializer.Deserialize<ResponseObject.Race>(stringContent);

            // duration is the only element that isn't deserialized correctly
            var splitString = stringContent.Split("totalMilliseconds");
            string millisecondsString = splitString[1]
                .Split("totalMinutes")[0]
                .Split(":")[1]
                .Split(",")[0];
            double durationMilliseconds = double.Parse(millisecondsString);

            Assert.Equal("Finished", raceResults.raceState);
            Assert.Equal(10, raceResults.numberOfLaps);
            Assert.True(new DateTime() < raceResults.startTime, "start time should be greater than 0");
            Assert.True(raceResults.startTime < raceResults.finishTime, "finish time should be greater than start time");
            Assert.True(durationMilliseconds > 0, "duration should be greater than 0");
            Assert.Equal(2, raceResults.finishOrder[0]);
            Assert.Equal(1, raceResults.finishOrder[1]);

            for (int i = 1; i < 3; i++)
            {
                Assert.Equal(i, raceResults.lapResults[i - 1].timerID);

                for (int lap = 1; lap <= 10; lap++)
                {
                    Assert.Contains(lap + ":", raceResults.lapResults[i - 1].laps[lap - 1]);
                    if (lap < 10)
                    {
                        Assert.Contains("00:01:0" + lap, raceResults.lapResults[i - 1].laps[lap - 1]);
                    }
                    else
                    {
                        Assert.Contains("00:01:" + lap, raceResults.lapResults[i - 1].laps[lap - 1]);
                    }
                }
            }

            Assert.Equal("success", raceResults.responseMessage);
        }
    }
}