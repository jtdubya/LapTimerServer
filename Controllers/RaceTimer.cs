using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LapTimerServer.Lib;
using System.Globalization;
using LapTimerServer.JsonObjects;
using Newtonsoft.Json;

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
            _logger.LogInformation("GET /GetMaxParticipants");
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
                _logger.LogError("RaceTimer/GetMaxParticipants Exception: " + error.Message);
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
            _logger.LogInformation("GET /SetMaxParticipants/" + newMax);
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
                _logger.LogError("RaceTimer/SetMaxParticipants Exception: " + error.Message);

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
            _logger.LogInformation("GET /Register/" + iPAddress);
            try
            {
                int registerCode = _raceManager.Register(iPAddress);
                ResponseObject.Register registerResponse = new ResponseObject.Register
                {
                    id = registerCode
                };

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
                _logger.LogError("RaceTimer/Register Exception: " + error.Message);
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
            _logger.LogInformation("GET /StartRace");
            try
            {
                string failPrefix = "StartRace fail: ";
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration,
                };

                if (_raceManager.GetParticipants().Count == 0)
                {
                    startResponse.responseMessage = failPrefix + "no lap timers registered.";
                }
                else
                {
                    bool success = _raceManager.StartRace();
                    if (success)
                    {
                        startResponse.responseMessage = "success";
                    }
                    else
                    {
                        startResponse.responseMessage = failPrefix + "cannot start race in state " + _raceManager.GetRaceState().ToString();
                    }
                }

                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/StartRace Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
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
            _logger.LogInformation("GET /GetRaceStartCountdownDuration");
            try
            {
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = "success",
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };
                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/GetRaceStartCountdownDuration Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
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
            _logger.LogInformation("GET /SetRaceStartCountdownDuration/" + newDuration);
            try
            {
                _raceManager.RaceStartCountdownDuration = newDuration;
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = "success",
                    raceStartCountdownDuration = _raceManager.RaceStartCountdownDuration
                };
                return Json(startResponse);
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/GetRaceStartCountdownDuration Exception: " + error.Message);
                ResponseObject.Start startResponse = new ResponseObject.Start
                {
                    responseMessage = error.Message,
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
            _logger.LogInformation("GET /GetTimeUntilRaceStart");
            ResponseObject.TimeUntilStart timeUntilStartResponse = new ResponseObject.TimeUntilStart
            {
                numberOfLaps = _raceManager.NumberOfLaps
            };

            try
            {
                timeUntilStartResponse.millisecondsUntilStart = _raceManager.GetMillisecondsUntilRaceStart();
                timeUntilStartResponse.responseMessage = "success";
                return Json(timeUntilStartResponse);
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/GetTimeUntilRaceStart Exception: " + error.Message);
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
            _logger.LogInformation("GET /GetRaceState");
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
                _logger.LogError("RaceTimer/GetRaceState Exception: " + error.Message);

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
        ///    Note that the last value ".3448" is ticks, there are 10,000 Ticks per millisecond
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
        public JsonResult AddLapResultAsString([FromBody] RequestObject.LapResult lapResult)
        {
            _logger.LogInformation("POST /AddLapResultAsString \n\tBODY: ipAddress:" + lapResult.ipAddress + " | lapTime: " + lapResult.lapTime);

            try
            {
                IPAddress ipAddress = IPAddress.Parse(lapResult.ipAddress);
                TimeSpan lapTime = TimeSpan.Parse(lapResult.lapTime, CultureInfo.InvariantCulture);
                _raceManager.AddLapResult(ipAddress, lapTime);
                return Json(new ResponseObject { responseMessage = "success" });
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/AddLapResult Exception: " + error.Message);

                if (error is KeyNotFoundException)
                {
                    return Json(new ResponseObject
                    {
                        responseMessage = "The given IP address '" + lapResult.ipAddress + "' is not registered."
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

        [HttpPost]
        public JsonResult AddLapResultInMilliseconds([FromBody] RequestObject.LapResultMilliseconds lapResultMilliseconds)
        {
            _logger.LogInformation("POST /AddLapResultInMilliseconds \n\tBODY: ipAddress:" + lapResultMilliseconds.ipAddress + " | lapTime: " + lapResultMilliseconds.lapTime);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(lapResultMilliseconds.ipAddress);
                TimeSpan lapTime = TimeSpan.FromMilliseconds(lapResultMilliseconds.lapTime);
                _raceManager.AddLapResult(ipAddress, lapTime);
                return Json(new ResponseObject { responseMessage = "success" });
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/AddLapResult Exception: " + error.Message);

                if (error is KeyNotFoundException)
                {
                    return Json(new ResponseObject
                    {
                        responseMessage = "The given IP address '" + lapResultMilliseconds.ipAddress + "' is not registered."
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

        [HttpGet("{id}")]
        public JsonResult GetLastRaceResultById(int id)
        {
            _logger.LogInformation("GET /GetLastRaceResultById");
            ResponseObject.RaceResultByID raceResult = new ResponseObject.RaceResultByID
            {
                id = id,
                responseMessage = "success"
            };

            try
            {
                if (_raceManager.GetRaceState() != RaceState.Finished)
                {
                    raceResult.responseMessage = "Race is not finished.";
                    return Json(raceResult);
                }

                Race lastRace = _raceManager.GetAllRaces().Last();
                int placeIndex = lastRace.GetFinishedParticipants().IndexOf(id); // -1 if not found
                Dictionary<int, List<Lap>> raceResults = _raceManager.GetCurrentRaceResults();
                bool idFound = raceResults.TryGetValue(id, out List<Lap> laps);

                if (placeIndex < 0 || !idFound)
                {
                    raceResult.responseMessage = "ID [" + id + "] was not found.";
                    return Json(raceResult);
                }

                raceResult.place = placeIndex + 1;
                Lap fastestLap = new Lap(0, new TimeSpan(0));
                TimeSpan totalTime = new TimeSpan();

                for (int i = 0; i < laps.Count; i++)
                {
                    if (i == 0)
                    {
                        fastestLap = laps[0];
                        fastestLap.Number = laps[0].Number;
                    }
                    else if (laps[i].Time < fastestLap.Time)
                    {
                        fastestLap = laps[i];
                    }
                    totalTime += laps[i].Time;
                }

                raceResult.overallTime = totalTime.ToString();
                raceResult.overallTimeMilliseconds = totalTime.TotalMilliseconds;
                raceResult.fastestLap = fastestLap.Time.ToString();
                raceResult.fastestLapMilliseconds = fastestLap.Time.TotalMilliseconds;
                raceResult.fastestLapNumber = fastestLap.Number;

                return Json(raceResult);
            }
            catch (Exception error)
            {
                _logger.LogError("RaceTimer/GetLastRaceResultById Exception: " + error.Message);

                raceResult.responseMessage = error.Message;

                return new JsonResult(raceResult)

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
            _logger.LogInformation("GET /GetCurrentRaceResults");
            try
            {
                List<Race> allRaces = _raceManager.GetAllRaces();

                if (allRaces.Count < 1)
                {
                    ResponseObject emptyResponse = new ResponseObject
                    {
                        responseMessage = "No races available."
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
                _logger.LogError("RaceTimer/GetCurrentRaceResults Exception: " + error.Message);

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