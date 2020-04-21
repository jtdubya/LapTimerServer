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
        public ActionResult GetRaceState()
        {
            try
            {
                RaceState state = _raceManager.GetRaceState();
                return Ok(state);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/GetRaceState Exception: " + error.Message);
                return BadRequest(error.Message);
            }
        }
    }
}