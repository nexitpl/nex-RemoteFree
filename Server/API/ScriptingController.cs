﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using nexRemoteFree.Server.Hubs;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Utilities;
using nexRemoteFree.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nexRemoteFree.Shared.Enums;
using nexRemoteFree.Server.Auth;

namespace nexRemoteFree.Server.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScriptingController : ControllerBase
    {
        private readonly IHubContext<AgentHub> _agentHubContext;

        private readonly IDataService _dataService;

        private readonly IExpiringTokenService _expiringTokenService;

        private readonly UserManager<nexRemoteFreeUser> _userManager;

        public ScriptingController(UserManager<nexRemoteFreeUser> userManager,
                                            IDataService dataService,
            IExpiringTokenService expiringTokenService,
            IHubContext<AgentHub> agentHub)
        {
            _dataService = dataService;
            _expiringTokenService = expiringTokenService;
            _userManager = userManager;
            _agentHubContext = agentHub;
        }

        [ServiceFilter(typeof(ApiAuthorizationFilter))]
        [HttpPost("[action]/{mode}/{deviceID}")]
        public async Task<ActionResult<ScriptResult>> ExecuteCommand(string mode, string deviceID)
        {
            if (!Enum.TryParse<ScriptingShell>(mode, true, out var shell))
            {
                return BadRequest("Nie można przeanalizować typu powłoki. Użyj PSCore, WinPS, Bash lub CMD.");
            }

            var command = string.Empty;
            using (var sr = new StreamReader(Request.Body))
            {
                command = await sr.ReadToEndAsync();
            }

            var userID = string.Empty;
            if (Request.HttpContext.User.Identity.IsAuthenticated)
            {
                var username = Request.HttpContext.User.Identity.Name;
                var user = await _userManager.FindByNameAsync(username);
                userID = user.Id;
                if (!_dataService.DoesUserHaveAccessToDevice(deviceID, user))
                {
                    return Unauthorized();
                }

            }

            Request.Headers.TryGetValue("OrganizationID", out var orgID);

            KeyValuePair<string, Device> connection = AgentHub.ServiceConnections.FirstOrDefault(x =>
                x.Value.OrganizationID == orgID &&
                x.Value.ID == deviceID);

            if (string.IsNullOrWhiteSpace(connection.Key))
            {
                return NotFound();
            }

            var requestID = Guid.NewGuid().ToString();
            var authToken = _expiringTokenService.GetToken(Time.Now.AddMinutes(AppConstants.ScriptRunExpirationMinutes));

            await _agentHubContext.Clients.Client(connection.Key).SendAsync("ExecuteCommandFromApi", shell, authToken, requestID, command, User?.Identity?.Name);

            var success = await TaskHelper.DelayUntilAsync(() => AgentHub.ApiScriptResults.TryGetValue(requestID, out _), TimeSpan.FromSeconds(30));
            if (!success)
            {
                return NotFound();
            }
            AgentHub.ApiScriptResults.TryGetValue(requestID, out var commandID);
            AgentHub.ApiScriptResults.Remove(requestID);
            var result = _dataService.GetScriptResult(commandID.ToString(), orgID);
            return result;
        }
    }
}
