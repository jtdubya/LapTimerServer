using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LapTimerServer.Lib;
using System.Globalization;

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
                    message = "success",
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
                    message = error.Message
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
                    message = "success",
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
                    message = error.Message
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
                ResponseObject.Register registerResponse = _raceManager.Register(iPAddress);
                return Json(registerResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/Register Exception: " + error.Message);
                ResponseObject.Register registerResponse = new ResponseObject.Register
                {
                    id = -1,
                    message = error.Message
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
                    message = "success",
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
                    message = error.Message,
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
                    message = "success",
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
                    message = error.Message,
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
                    message = "success",
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
                    message = error.Message,
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
            try
            {
                long time = _raceManager.GetMillisecondsUntilRaceStart();
                return Ok(time);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetTimeUntilRaceStart Exception: " + error.Message);
                return BadRequest(error.Message);
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
                    message = "success",
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
                    message = "error.Message",
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
                return Json(new ResponseObject { message = "success" });
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/AddLapResult Exception: " + error.Message);

                if (error is KeyNotFoundException)
                {
                    return Json(new ResponseObject
                    {
                        message = "The given ip address '" + lapResult.ipAddress + "' is not registered."
                    });
                }

                ResponseObject response = new ResponseObject
                {
                    message = error.Message,
                };

                return new JsonResult(response)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }
}