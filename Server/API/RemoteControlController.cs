using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using nexRemote.Server.Attributes;
using nexRemote.Server.Hubs;
using nexRemote.Server.Models;
using nexRemote.Server.Services;
using nexRemote.Shared.Utilities;
using nexRemote.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using nexRemote.Server.Auth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace nexRemote.Server.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemoteControlController : ControllerBase
    {
        public RemoteControlController(IDataService dataService,
            IHubContext<AgentHub> agentHub,
            IApplicationConfig appConfig,
            SignInManager<RemotelyUser> signInManager)
        {
            DataService = dataService;
            AgentHubContext = agentHub;
            AppConfig = appConfig;
            SignInManager = signInManager;
        }

        public IDataService DataService { get; }
        public IHubContext<AgentHub> AgentHubContext { get; }
        public IApplicationConfig AppConfig { get; }
        public SignInManager<RemotelyUser> SignInManager { get; }

        [HttpGet("{deviceID}")]
        [ServiceFilter(typeof(ApiAuthorizationFilter))]
        public async Task<IActionResult> Get(string deviceID)
        {
            Request.Headers.TryGetValue("OrganizationID", out var orgID);
            return await InitiateRemoteControl(deviceID, orgID);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RemoteControlRequest rcRequest)
        {
            if (!AppConfig.AllowApiLogin)
            {
                return NotFound();
            }

            var orgId = DataService.GetUserByNameWithOrg(rcRequest.Email)?.OrganizationID;

            var result = await SignInManager.PasswordSignInAsync(rcRequest.Email, rcRequest.Password, false, true);
            if (result.Succeeded &&
                DataService.DoesUserHaveAccessToDevice(rcRequest.DeviceID, DataService.GetUserByNameWithOrg(rcRequest.Email)))
            {
                DataService.WriteEvent($"Logowanie API powiodło się dla {rcRequest.Email}.", orgId);
                return await InitiateRemoteControl(rcRequest.DeviceID, orgId);
            }
            else if (result.IsLockedOut)
            {
                DataService.WriteEvent($"Logowanie do API nie powiodło się z powodu blokady dla {rcRequest.Email}.", orgId);
                return Unauthorized("Konto jest zablokowane.");
            }
            else if (result.RequiresTwoFactor)
            {
                DataService.WriteEvent($"Logowanie do API nie powiodło się z powodu 2FA dla {rcRequest.Email}.", orgId);
                return Unauthorized("Konto wymaga uwierzytelnienia dwuskładnikowego.");
            }
            DataService.WriteEvent($"Logowanie API nie powiodło się z powodu nieudanej próby dla {rcRequest.Email}.", orgId);
            return BadRequest();
        }

        private async Task<IActionResult> InitiateRemoteControl(string deviceID, string orgID)
        {
            var targetDevice = AgentHub.ServiceConnections.FirstOrDefault(x =>
                                    x.Value.OrganizationID == orgID &&
                                    x.Value.ID.ToLower() == deviceID.ToLower());

            if (targetDevice.Value != null)
            {
                if (User.Identity.IsAuthenticated &&
                   !DataService.DoesUserHaveAccessToDevice(targetDevice.Value.ID, DataService.GetUserByNameWithOrg(User.Identity.Name)))
                {
                    return Unauthorized();
                }


                var currentUsers = CasterHub.SessionInfoList.Count(x => x.Value.OrganizationID == orgID);
                if (currentUsers >= AppConfig.RemoteControlSessionLimit)
                {
                    return BadRequest("Istnieje już maksymalna liczba aktywnych sesji zdalnego sterowania dla Twojej organizacji.");
                }

                var existingSessions = CasterHub.SessionInfoList
                    .Where(x => x.Value.DeviceID == targetDevice.Value.ID)
                    .Select(x => x.Key)
                    .ToList();

                await AgentHubContext.Clients.Client(targetDevice.Key).SendAsync("RemoteControl", Request.HttpContext.Connection.Id, targetDevice.Key);

                bool remoteControlStarted()
                {
                    return !CasterHub.SessionInfoList.Values
                        .Where(x => x.DeviceID == targetDevice.Value.ID)
                        .All(x => existingSessions.Contains(x.CasterSocketID));
                };

                if (!await TaskHelper.DelayUntilAsync(remoteControlStarted, TimeSpan.FromSeconds(30)))
                {
                    return StatusCode(408, "Proces zdalnego sterowania nie rozpoczął się na czas na zdalnym urządzeniu.");
                }
                else
                {
                    var rcSession = CasterHub.SessionInfoList.Values.LastOrDefault(x => x.DeviceID == targetDevice.Value.ID && !existingSessions.Contains(x.CasterSocketID));
                    var otp = RemoteControlFilterAttribute.GetOtp(targetDevice.Value.ID);
                    return Ok($"{HttpContext.Request.Scheme}://{Request.Host}/RemoteControl?casterID={rcSession.CasterSocketID}&serviceID={targetDevice.Key}&fromApi=true&otp={Uri.EscapeDataString(otp)}");
                }
            }
            else
            {
                return BadRequest("Nie można znaleźć urządzenia docelowego.");
            }
        }
    }
}
