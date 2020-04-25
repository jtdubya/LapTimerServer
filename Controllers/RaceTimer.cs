using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LapTimerServer.Lib;
using System.Globalization;
using LapTimerServer.JsonObjects;

namespace LapTimerServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class RaceTimer : Controller
    {
        private readonly ILogger<RaceTimer> _logger;
        private readonly RaceManager _raceManager;

        public RaceTimer(ILogger<RaceTimer> logger, RaceManager raceManager)
        {
            _logger = logger;
            _raceManager = raceManager;
        }

        [HttpGet]
        public JsonResult GetMaxParticipants()
        {
            try
            {
                int max = _raceManager.GetMaxParticipants();
                ResponseObject.Participants response = new ResponseObject.Participants
                {
                    responseMessage = "success",
                    maxParticipants = max
                };
                return new JsonResult(response);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetMaxParticipants Exception: " + error.Message);
                ResponseObject.Participants response = new ResponseObject.Participants
                {
                    maxParticipants = -1,
                    responseMessage = error.Message
                };

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        [HttpGet("{newMax}")]
        public JsonResult SetMaxParticipants(int newMax)
        {
            try
            {
                _raceManager.SetMaxParticipants(newMax);
                ResponseObject.Participants response = new ResponseObject.Participants
                {
                    responseMessage = "success",
                    maxParticipants = _raceManager.GetMaxParticipants()
                };
                return new JsonResult(response);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/SetMaxParticipants Exception: " + error.Message);

                ResponseObject.Participants response = new ResponseObject.Participants
                {
                    maxParticipants = _raceManager.GetMaxParticipants(),
                    responseMessage = error.Message
                };

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// GET: /RaceTimer/Register/IP Address (ex: /RaceTimer/Register/10.0.0.1)
        /// IP address is used as unique identifier for each device
        /// </summary>
        /// <param name="iPAddress">the ipv4 address of the race timer</param>
        /// <returns>ID of new or existing race timer or an error message</returns>
        [HttpGet("{iPAddress}")]
        public JsonResult Register(string iPAddress)
        {
            try
            {
                int registerCode = _raceManager.Register(iPAddress);
                ResponseObject.Register registerResponse = new ResponseObject.Register();
                registerResponse.id = registerCode;

                if (registerCode > 0)
                {
                    registerResponse.responseMessage = "success";
                }
                else
                {
                    registerResponse.responseMessage = "Registration closed. ";

                    if (registerCode == -1)
                    {
                        registerResponse.responseMessage += "Wait until next registration period.";
                    }
                    else if (registerCode == -2)
                    {
                        registerResponse.responseMessage += "Max participants reached.";
                    }
                }

                return Json(registerResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/Register Exception: " + error.Message);
                ResponseObject.Register registerResponse = new ResponseObject.Register
                {
                    id = -1,
                    responseMessage = error.Message
                };

                return new JsonResult(registerResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        [HttpGet]
        public JsonResult StartRace()
        {
            try
            {
                _raceManager.StartRace();
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = "success",
                    millisSecondsUntilRaceStart = _raceManager.RaceStartCountdownDuration,
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };
                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/StartRace Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
                    millisSecondsUntilRaceStart = _raceManager.RaceStartCountdownDuration,
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };

                return new JsonResult(startResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        [HttpGet]
        public JsonResult GetRaceStartCountdownDuration()
        {
            try
            {
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = "success",
                    millisSecondsUntilRaceStart = _raceManager.GetMillisecondsUntilRaceStart(),
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };
                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetRaceStartCountdownDuration Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
                    millisSecondsUntilRaceStart = _raceManager.GetMillisecondsUntilRaceStart(),
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };

                return new JsonResult(startResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        [HttpGet("{newDuration}")]
        public JsonResult SetRaceStartCountdownDuration(long newDuration)
        {
            try
            {
                _raceManager.RaceStartCountdownDuration = newDuration;
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = "success",
                    millisSecondsUntilRaceStart = _raceManager.GetMillisecondsUntilRaceStart(),
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };
                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetRaceStartCountdownDuration Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
                    millisSecondsUntilRaceStart = _raceManager.GetMillisecondsUntilRaceStart(),
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };

                return new JsonResult(startResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        [HttpGet]
        public ActionResult GetTimeUntilRaceStart()
        {
            ResponseObject.TimeUntilStart timeUntilStartResponse = new ResponseObject.TimeUntilStart();
            timeUntilStartResponse.numberOfLaps = _raceManager.NumberOfLaps;

            try
            {
                timeUntilStartResponse.millisecondsUntilStart = _raceManager.GetMillisecondsUntilRaceStart();
                timeUntilStartResponse.responseMessage = "success";
                return Json(timeUntilStartResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetTimeUntilRaceStart Exception: " + error.Message);
                timeUntilStartResponse.responseMessage = error.Message;
                timeUntilStartResponse.millisecondsUntilStart = -1;

                return new JsonResult(timeUntilStartResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };
            }
        }

        [HttpGet]
        public JsonResult GetRaceState()
        {
            try
            {
                RaceState state = _raceManager.GetRaceState();

                ResponseObject.State stateResponse = new ResponseObject.State
                {
                    responseMessage = "success",
                    state = state,
                    stateName = state.ToString()
                };
                return Json(stateResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetRaceState Exception: " + error.Message);

                ResponseObject.State stateResponse = new ResponseObject.State
                {
                    responseMessage = "error.Message",
                    state = 0,
                    stateName = "error getting state"
                };

                return new JsonResult(stateResponse)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Adds a lap result. Lap results must be added sequentially.
        /// </summary>
        /// <remarks>
        ///   Provided Lap Time string --> Parsed TimeSpan \
        /// \
        ///    6 --> 6.00:00:00 \
        ///    6:12 --> 06:12:00 \
        ///    6:12:14 --> 06:12:14 \
        ///    6:12:14:45 --> 6.12:14:45 \
        ///    6.12:14:45 --> 6.12:14:45 \
        ///    6:12:14:45.3448 --> 6.12:14:45.3448000
        ///
        ///     Sample Request:
        ///
        ///     POST api/v1/RaceTimer/AddLapResult
        ///     {
        ///         "ipAddress": "192.168.1.20",
        ///         "lapTime": "0:1:14:56"
        ///     }
        ///
        /// </remarks>
        /// <param name="lapResult"></param>
        /// <returns>Success or error message</returns>
        [HttpPost]
        public JsonResult AddLapResult([FromBody] RequestObject.LapResult lapResult)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(lapResult.ipAddress);
                TimeSpan lapTime = TimeSpan.Parse(lapResult.lapTime, CultureInfo.InvariantCulture);
                _raceManager.AddLapResult(ipAddress, lapTime);
                return Json(new ResponseObject { responseMessage = "success" });
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/AddLapResult Exception: " + error.Message);

                if (error is KeyNotFoundException)
                {
                    return Json(new ResponseObject
                    {
                        responseMessage = "The given ip address '" + lapResult.ipAddress + "' is not registered."
                    });
                }

                ResponseObject response = new ResponseObject
                {
                    responseMessage = error.Message,
                };

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        //   Swagger fails to parse the response object so it's added in comments
        /// <summary>
        /// Get results for the current race (can be in progress or finished)
        /// Note: null time will be "0001-01-01T00:00:00"
        /// </summary>
        /// <remarks>
        /// Sample Response:
        ///
        ///     GET api/v1/RaceTimer/GetCurrentRaceResults
        ///     {
        ///        "raceState": "Finished",
        ///        "numberOfLaps": 10,
        ///        "startTime": "2020-04-22T17:40:20.2150886-05:00",
        ///        "finishTime": "2020-04-22T17:41:20.0329489-05:00",
        ///        "duration": {
        ///            "ticks": 598178603,
        ///            "days": 0,
        ///            "hours": 0,
        ///            "milliseconds": 817,
        ///            "minutes": 0,
        ///            "seconds": 59,
        ///            "totalDays": 0.0006923363460648148,
        ///            "totalHours": 0.016616072305555556,
        ///            "totalMilliseconds": 59817.8603,
        ///            "totalMinutes": 0.9969643383333333,
        ///            "totalSeconds": 59.8178603
        ///        },
        ///        "finishOrder": [
        ///            1,
        ///            2
        ///        ],
        ///        "lapResults": [
        ///            {
        ///                "timerID": 1,
        ///                "laps": [
        ///                    "1: 00:01:10",
        ///                    "2: 00:01:10",
        ///                    "3: 00:01:10",
        ///                    "4: 00:01:10",
        ///                    "5: 00:01:10",
        ///                    "6: 00:01:10",
        ///                    "7: 00:01:10",
        ///                    "8: 00:01:10",
        ///                    "9: 00:01:10",
        ///                    "10: 00:01:10"
        ///                ]
        ///        },
        ///            {
        ///                "timerID": 2,
        ///                "laps": [
        ///                    "1: 00:01:10",
        ///                    "2: 00:01:10",
        ///                    "3: 00:01:10",
        ///                    "4: 00:01:10",
        ///                    "5: 00:01:10",
        ///                    "6: 00:01:10",
        ///                    "7: 00:01:10",
        ///                    "8: 00:01:10",
        ///                    "9: 00:01:10",
        ///                    "10: 00:01:10"
        ///                ]
        ///             }
        ///       ],
        ///       "responseMessage": "success"
        ///    }
        /// </remarks>
        /// <returns>ResponseObject.Race</returns>
        [HttpGet]
        public JsonResult GetCurrentRaceResults()
        {
            try
            {
                List<Race> allRaces = _raceManager.GetAllRaces();

                if (allRaces.Count < 1)
                {
                    ResponseObject emptyResponse = new ResponseObject
                    {
                        responseMessage = "no races available"
                    };
                    return Json(emptyResponse);
                }

                Race currentRace = allRaces.Last();
                ResponseObject.Race raceResponse = new ResponseObject.Race { responseMessage = "success" };
                raceResponse.raceState = _raceManager.GetRaceState().ToString();
                raceResponse.numberOfLaps = currentRace.GetNumberOfLaps();
                raceResponse.startTime = currentRace.GetStartTime();

                if (_raceManager.GetRaceState() == RaceState.Finished)
                {
                    raceResponse.finishTime = currentRace.GetFinishTime();
                }
                raceResponse.duration = currentRace.GetDuration(); ;
                raceResponse.finishOrder = currentRace.GetFinishedParticipants();

                Dictionary<int, List<Lap>> raceResults = _raceManager.GetCurrentRaceResults();
                List<ResponseObject.LapResult> lapResults = new List<ResponseObject.LapResult>();
                foreach (var result in raceResults)
                {
                    ResponseObject.LapResult lapResult = new ResponseObject.LapResult
                    {
                        timerID = result.Key,
                        laps = new List<string>()
                    };

                    foreach (Lap lap in result.Value)
                    {
                        lapResult.laps.Add(lap.ToString());
                    }
                    lapResults.Add(lapResult);
                }

                raceResponse.lapResults = lapResults;
                return Json(raceResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetCurrentRaceResults Exception: " + error.Message);

                ResponseObject response = new ResponseObject
                {
                    responseMessage = error.Message,
                };

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }
}