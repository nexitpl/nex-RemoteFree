﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using nexRemoteFree.Server.Hubs;
using nexRemoteFree.Server.Services;
using nexRemoteFree.Shared.Enums;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nexRemoteFree.Server.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentUpdateController : ControllerBase
    {
        private static readonly MemoryCache _downloadingAgents = new(new MemoryCacheOptions()
        { ExpirationScanFrequency = TimeSpan.FromSeconds(10) });


        public AgentUpdateController(IWebHostEnvironment hostingEnv,
            IDataService dataService,
            IApplicationConfig appConfig,
            IHubContext<AgentHub> agentHubContext)
        {
            HostEnv = hostingEnv;
            DataService = dataService;
            AppConfig = appConfig;
            AgentHubContext = agentHubContext;
        }

        private IDataService DataService { get; }
        private IApplicationConfig AppConfig { get; }
        private IHubContext<AgentHub> AgentHubContext { get; }
        private IWebHostEnvironment HostEnv { get; }


        [HttpGet("[action]/{downloadId}")]
        public ActionResult ClearDownload(string downloadId)
        {
            DataService.WriteEvent($"Usuwanie ID pobierania {downloadId}.", EventType.Debug, null);
            _downloadingAgents.Remove(downloadId);
            return Ok();
        }

        [HttpGet("[action]/{platform}/{downloadId}")]
        public async Task<ActionResult> DownloadPackage(string platform, string downloadId)
        {
            try
            {
                var remoteIp = Request?.HttpContext?.Connection?.RemoteIpAddress.ToString();

                if (await CheckForDeviceBan(remoteIp))
                {
                    return BadRequest();
                }

                var startWait = DateTimeOffset.Now;

                while (_downloadingAgents.Count >= AppConfig.MaxConcurrentUpdates)
                {
                    await Task.Delay(new Random().Next(100, 10000));

                    // A get operation is necessary to evaluate item eviction.
                    _downloadingAgents.TryGetValue(string.Empty, out _);
                }

                var entryExpirationTime = TimeSpan.FromMinutes(3);
                var tokenExpirationTime = entryExpirationTime.Add(TimeSpan.FromSeconds(15));

                var expirationToken = new CancellationChangeToken(
                    new CancellationTokenSource(tokenExpirationTime).Token);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(entryExpirationTime)
                    .AddExpirationToken(expirationToken);

                _downloadingAgents.Set(downloadId, string.Empty, cacheOptions);

                var waitTime = DateTimeOffset.Now - startWait;
                DataService.WriteEvent($"Pobieranie rozpoczęło się po czasie oczekiwania {waitTime}.  " + 
                    $"ID: {downloadId}. " +
                    $"IP: {remoteIp}. " +
                    $"Aktualny Download: {_downloadingAgents.Count}.  Max Dozwolone: {AppConfig.MaxConcurrentUpdates}", EventType.Debug, null);


                string filePath;

                switch (platform.ToLower())
                {
                    case "win-x64":
                        filePath = Path.Combine(HostEnv.WebRootPath, "Content", "nex-RemoteFree-Win10-x64.zip");
                        break;
                    case "win-x86":
                        filePath = Path.Combine(HostEnv.WebRootPath, "Content", "nex-RemoteFree-Win10-x86.zip");
                        break;
                    case "linux":
                        filePath = Path.Combine(HostEnv.WebRootPath, "Content", "nex-RemoteFree-Linux.zip");
                        break;
                    case "macos-x64":
                        filePath = Path.Combine(HostEnv.WebRootPath, "Content", "nex-RemoteFree-MacOS-x64.zip");
                        break;
                    default:
                        DataService.WriteEvent($"Zażądano nieznanej platformy w {nameof(AgentUpdateController)}. " +
                            $"Platform: {platform}. " +
                            $"IP: {remoteIp}.",
                            EventType.Warning,
                            null);
                        return BadRequest();
                }

                var fileStream = System.IO.File.OpenRead(filePath);

                return File(fileStream, "application/octet-stream", "nex-RemoteFreeUpdate.zip");
            }
            catch (Exception ex)
            {
                _downloadingAgents.Remove(downloadId);
                DataService.WriteEvent(ex, null);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private async Task<bool> CheckForDeviceBan(string deviceIp)
        {
            if (string.IsNullOrWhiteSpace(deviceIp))
            {
                return false;
            }

            if (AppConfig.BannedDevices.Contains(deviceIp))
            {
                DataService.WriteEvent($"IP urządzenia ({deviceIp}) jest zablokowane.  Wysyłam komendę deinstalacji.", null);

                var bannedDevices = AgentHub.ServiceConnections.Where(x => x.Value.PublicIP == deviceIp);
                foreach (var bannedDevice in bannedDevices)
                {
                    // TODO: Remove when devices have been removed.
                    var command = "sc delete nex-RemoteFree_Service & taskkill /im nex-RemoteFree_Agent.exe /f";
                    await AgentHubContext.Clients.Client(bannedDevice.Key).SendAsync("ExecuteCommand", 
                        "cmd", 
                        command,
                        Guid.NewGuid().ToString(), 
                        Guid.NewGuid().ToString());

                    await AgentHubContext.Clients.Client(bannedDevice.Key).SendAsync("UninstallAgent");    
                }

                return true;
            }

            return false;
        }
    }
}
