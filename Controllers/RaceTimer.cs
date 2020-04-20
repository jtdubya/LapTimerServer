using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LapTimerServer.Lib;

namespace LapTimerServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class RaceTimer : Controller
    {
        private readonly ILogger<RaceTimer> m_logger;
        private readonly RaceManager m_raceManager;

        public RaceTimer(ILogger<RaceTimer> logger, RaceManager raceManager)
        {
            m_logger = logger;
            m_raceManager = raceManager;
        }

        /// <summary>
        /// GET: /RaceTimer/Register/IP Address (ex: /RaceTimer/Register/10.0.0.1)
        /// IP address is used as unique identifier for each device
        /// </summary>
        /// <param name="iPAddress">the ipv4 address of the race timer</param>
        /// <returns>ID of new or existing race timer or an error message</returns>
        [HttpGet("{iPAddress}")]
        public ActionResult Register(string iPAddress)
        {
            try
            {
                int id = m_raceManager.Register(iPAddress);
                return Ok(id);
            }
            catch (Exception error)
            {
                m_logger.LogInformation("RaceTimer/Register Exception: " + error.Message);
                return BadRequest(error.Message);
            }
        }

        [HttpGet]
        public ActionResult StartRace()
        {
            try
            {
                m_raceManager.StartRace();
                return Ok(m_raceManager.RaceStartCountdownDuration);
            }
            catch (Exception error)
            {
                m_logger.LogInformation("RaceTimer/StartRace Exception: " + error.Message);
                return BadRequest(error.Message);
            }
        }

        [HttpGet]
        public ActionResult GetTimeUntilRaceStart()
        {
            try
            {
                long time = m_raceManager.GetMillisecondsUntilRaceStart();
                return Ok(time);
            }
            catch (Exception error)
            {
                m_logger.LogInformation("RaceTimer/GetTimeUntilRaceStart Exception: " + error.Message);
                return BadRequest(error.Message);
            }
        }

        [HttpGet]
        public ActionResult GetRaceState()
        {
            try
            {
                RaceState state = m_raceManager.GetRaceState();
                return Ok(state);
            }
            catch (Exception error)
            {
                m_logger.LogInformation("RaceTimer/GetRaceState Exception: " + error.Message);
                return BadRequest(error.Message);
            }
        }
    }
}