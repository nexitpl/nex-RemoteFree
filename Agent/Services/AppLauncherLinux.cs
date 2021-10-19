using Microsoft.AspNetCore.SignalR.Client;
using nexRemote.Agent.Interfaces;
using nexRemote.Shared.Models;
using nexRemote.Shared.Services;
using nexRemote.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace nexRemote.Agent.Services
{

    public class AppLauncherLinux : IAppLauncher
    {
        private readonly string _rcBinaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nex-Remote", EnvironmentHelper.DesktopExecutableFileName);
        private readonly IProcessInvoker _processInvoker;
        private readonly ConnectionInfo _connectionInfo;

        public AppLauncherLinux(ConfigService configService, IProcessInvoker processInvoker)
        {
            _processInvoker = processInvoker;
            _connectionInfo = configService.GetConnectionInfo();
        }


        public async Task<int> LaunchChatService(string orgName, string requesterID, HubConnection hubConnection)
        {
            try
            {
                if (!File.Exists(_rcBinaryPath))
                {
                    await hubConnection.SendAsync("DisplayMessage",
                        "Nie znaleziono pliku wykonywalnego czatu na urządzeniu docelowym.", 
                        "Executable not found on device.", 
                        "bg-danger",
                        requesterID);
                }


                // Start Desktop app.
                await hubConnection.SendAsync("DisplayMessage", $"Uruchamiam usługę czatu.", "Uruchamiam usługę czatu.", "bg-success", requesterID);
                var args = $"{_rcBinaryPath} " +
                    $"-mode Chat " +
                    $"-requester \"{requesterID}\" " +
                    $"-organization \"{orgName}\" " +
                    $"-host \"{_connectionInfo.Host}\" " +
                    $"-orgid \"{_connectionInfo.OrganizationID}\"";
                return StartLinuxDesktopApp(args);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                await hubConnection.SendAsync("DisplayMessage", "Nie udało się uruchomić usługi czatu na urządzeniu docelowym.", "Nie udało się uruchomić usługi czatu.", "bg-danger", requesterID);
            }
            return -1;
        }

        public async Task LaunchRemoteControl(int targetSessionId, string requesterID, string serviceID, HubConnection hubConnection)
        {
            try
            {
                if (!File.Exists(_rcBinaryPath))
                {
                    await hubConnection.SendAsync("DisplayMessage",
                        "Nie znaleziono pliku wykonywalnego kontroli zdalnej na urządzeniu docelowym.",
                        "Nie znaleziono pliku wykonywalnego na urządzeniu.", 
                        "bg-danger", 
                        requesterID);
                    return;
                }


                // Start Desktop app.
                await hubConnection.SendAsync("DisplayMessage", "Uruchamianie kontroli zdalnej.", "Uruchamianie kontroli zdalnej.",  "bg-success", requesterID);
                var args = $"{_rcBinaryPath} " +
                    $"-mode Unattended " +
                    $"-requester \"{requesterID}\" " +
                    $"-serviceid \"{serviceID}\" " +
                    $"-deviceid {_connectionInfo.DeviceID} " +
                    $"-host \"{_connectionInfo.Host}\" " +
                    $"-orgid \"{_connectionInfo.OrganizationID}\"";
                StartLinuxDesktopApp(args);
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                await hubConnection.SendAsync("DisplayMessage", "Nie udało się uruchomić kontroli zdalnej na urządzeniu docelowym.", "Nie udało się uruchomić pilota.", "bg-danger", requesterID);
            }
        }
        public async Task RestartScreenCaster(List<string> viewerIDs, string serviceID, string requesterID, HubConnection hubConnection, int targetSessionID = -1)
        {
            try
            {
                // Start Desktop app.                 
                var args = $"{_rcBinaryPath} " +
                    $"-mode Unattended " +
                    $"-requester \"{requesterID}\" " +
                    $"-serviceid \"{serviceID}\" " +
                    $"-deviceid {_connectionInfo.DeviceID} " +
                    $"-host \"{_connectionInfo.Host}\" " +
                    $"-orgid \"{_connectionInfo.OrganizationID}\" " +
                    $"-relaunch true " +
                    $"-viewers {string.Join(",", viewerIDs)}";
                StartLinuxDesktopApp(args);
            }
            catch (Exception ex)
            {
                await hubConnection.SendAsync("SendConnectionFailedToViewers", viewerIDs);
                Logger.Write(ex);
                throw;
            }
        }

        private int StartLinuxDesktopApp(string args)
        {
            var xauthority = GetXorgAuth();

            var display = ":0";
            var whoString = _processInvoker.InvokeProcessOutput("who", "")?.Trim();
            var username = "";

            if (!string.IsNullOrWhiteSpace(whoString))
            {
                try
                {
                    var whoLine = whoString
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .First();

                    var whoSplit = whoLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    username = whoSplit[0];
                    display = whoSplit.Last().TrimStart('(').TrimEnd(')');
                    xauthority = $"/home/{username}/.Xauthority";
                    args = $"-u {username} {args}";
                }
                catch (Exception ex)
                {
                    Logger.Write(ex);
                }
            }

            var psi = new ProcessStartInfo()
            {
                FileName = "sudo",
                Arguments = args
            };

            psi.Environment.Add("DISPLAY", display);
            psi.Environment.Add("XAUTHORITY", xauthority);
            Logger.Write($"Próba uruchomienia programu screen cast z nazwą użytkownika {username}, xauthority {xauthority}, display {display}, and args {args}.");
            return Process.Start(psi).Id;
        }

        private string GetXorgAuth()
        {
            try
            {
                var processes = _processInvoker.InvokeProcessOutput("ps", "-eaf")?.Split(Environment.NewLine);
                if (processes?.Length > 0)
                {
                    var xorgLine = processes.FirstOrDefault(x => x.Contains("xorg", StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(xorgLine))
                    {
                        var xorgSplit = xorgLine?.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                        var authIndex = xorgSplit?.IndexOf("-auth");
                        if (authIndex > -1 && xorgSplit?.Count >= authIndex + 1)
                        {
                            var auth = xorgSplit[(int)authIndex + 1];
                            if (!string.IsNullOrWhiteSpace(auth))
                            {
                                return auth;
                            }
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }
    }
}
