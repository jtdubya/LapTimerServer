# LapTimerServer
This is a backend server with API for the Lap Timer system. It contain an ASP.Net Core 3.1 Web API that works with IoT lap timers like [this one](https://github.com/jtdubya/IoTLapTimer) 


## Getting started
* Build and run the project
* Swagger is accessible from the root URL to view and test endpoints
* Check out the [integration tests](Tests/ControllerIntegrationTests/RaceTimerIntegrationTests.cs) to see the RaceTimer controller behavior

# Lap Time Announcer
The lap time announcer plays audio of the lap time up to one minute.

# Race Manager
The [race manager](Lib/RaceManager.cs) is the core of the application that controls everything.

### Current Features
1. Register lap timers
1. Start a race
1. Add lap results
1. Get current race results
1. Get current race results for a specific lap timer

### Race Manager States
1. Registration
    * New lap timers can register with the race manager in this state
    * Transitions to `StartCountDown` by a call to the `/StartRace` endpoint
1. StartCountdown
    * Counts down to a race start. The countdown gives the IoT timers a chance to synchronize the start time
    * Transitions to `InProgress` when the countdown expires
1. InProgress
    * Transitions to `FinishCountdown` once the first timer has finished the race
1. FinishCountdown
    * Transitions to `Finished` once either all lap timers have finished or the countdown has expired
1. Finished
    * Transitions to `StartCountDown` by a call to the `/StartRace` endpoint


