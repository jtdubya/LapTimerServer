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

        /// <summary>
        /// GET: /RaceTimer/Register/IP Address (ex: /RaceTimer/Register/10.0.0.1)
        /// IP address is used as unique identifier for each device
        /// </summary>
        /// <param name="iPAddress">the ipv4 address of the race timer</param>
        /// <returns>ID of new or existing race timer or an error message</returns>
        [HttpGet("{iPAddress}")]
        [Produces("application/json")]
        public JsonResult Register(string iPAddress)
        {
            try
            {
                ResponseObjects.Register registerResponse = _raceManager.Register(iPAddress);
                return Json(registerResponse);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/Register Exception: " + error.Message);
                ResponseObjects.Register registerResponse = new ResponseObjects.Register
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
        public ActionResult StartRace()
        {
            try
            {
                _raceManager.StartRace();
                return Ok(_raceManager.RaceStartCountdownDuration);
            }
            catch (Exception error)
            {
                _logger.LogInformation("RaceTimer/StartRace Exception: " + error.Message);
                return BadRequest(error.Message);
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