using Microsoft.AspNetCore.SignalR.Client;
using nexRemote.Agent.Interfaces;
using nexRemote.Shared.Models;
using nexRemote.Shared.Utilities;
using nexRemote.Shared.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace nexRemote.Agent.Services
{

    public class AppLauncherWin : IAppLauncher
    {
        private readonly string _rcBinaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Desktop", EnvironmentHelper.DesktopExecutableFileName);

        public AppLauncherWin(ConfigService configService)
        {
            ConnectionInfo = configService.GetConnectionInfo();
        }

        private ConnectionInfo ConnectionInfo { get; }

        public async Task<int> LaunchChatService(string orgName, string requesterID, HubConnection hubConnection)
        {
            try
            {
                if (!File.Exists(_rcBinaryPath))
                {
                    await hubConnection.SendAsync("DisplayMessage", "Nie znaleziono pliku wykonywalnego czatu na urządzeniu docelowym.", "Nie znaleziono pliku wykonywalnego na urządzeniu.", "bg-danger", requesterID);
                }


                // Start Desktop app.
                await hubConnection.SendAsync("DisplayMessage", $"Uruchamiam usługę czatu.", "Uruchamiam usługę czatu.", "bg-success", requesterID);
                if (WindowsIdentity.GetCurrent().IsSystem)
                {
                    var result = Win32Interop.OpenInteractiveProcess($"{_rcBinaryPath} " +
                            $"-mode Chat " +
                            $"-requester \"{requesterID}\" " +
                            $"-organization \"{orgName}\" " +
                            $"-host \"{ConnectionInfo.Host}\" " +
                            $"-orgid \"{ConnectionInfo.OrganizationID}\"",
                        targetSessionId: -1,
                        forceConsoleSession: false,
                        desktopName: "default",
                        hiddenWindow: false,
                        out var procInfo);
                    if (!result)
                    {
                        await hubConnection.SendAsync("DisplayMessage",
                            "Nie udało się uruchomić usługi czatu na urządzeniu docelowym.",
                            "Nie udało się uruchomić usługi czatu.", 
                            "bg-danger",
                            requesterID);
                    }
                    else
                    {
                        return procInfo.dwProcessId;
                    }
                }
                else
                {
                    return Process.Start(_rcBinaryPath, 
                        $"-mode Chat " +
                        $"-requester \"{requesterID}\" " +
                        $"-organization \"{orgName}\" " +
                         $"-host \"{ConnectionInfo.Host}\" " +
                        $"-orgid \"{ConnectionInfo.OrganizationID}\"").Id;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                await hubConnection.SendAsync("DisplayMessage",
                    "Nie udało się uruchomić usługi czatu na urządzeniu docelowym.",
                    "Nie udało się uruchomić usługi czatu.",
                    "bg-danger",
                    requesterID);
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
                        "Nie znaleziono pliku wykonywalnego zdalnego sterowania na urządzeniu docelowym.",
                        "Nie znaleziono pliku wykonywalnego na urządzeniu.", 
                        "bg-danger",
                        requesterID);
                    return;
                }


                // Start Desktop app.
                await hubConnection.SendAsync("DisplayMessage", 
                    "Uruchamianie kontroli zdalnej.",
                    "Uruchamianie kontroli zdalnej.",
                    "bg-success",
                    requesterID);
                if (WindowsIdentity.GetCurrent().IsSystem)
                {
                    var result = Win32Interop.OpenInteractiveProcess(_rcBinaryPath +
                            $" -mode Unattended" +
                            $" -requester \"{requesterID}\"" +
                            $" -serviceid \"{serviceID}\"" +
                            $" -deviceid {ConnectionInfo.DeviceID}" +
                            $" -host {ConnectionInfo.Host}" +
                            $" -orgid \"{ConnectionInfo.OrganizationID}\"",
                        targetSessionId: targetSessionId,
                        forceConsoleSession: Shlwapi.IsOS(OsType.OS_ANYSERVER) && targetSessionId == -1,
                        desktopName: "default",
                        hiddenWindow: true,
                        out _);
                    if (!result)
                    {
                        await hubConnection.SendAsync("DisplayMessage",
                            "Nie udało się uruchomić kontroli zdalnej na urządzeniu docelowym.",
                            "Nie udało się uruchomić kontroli zdalnej.",
                            "bg-danger",
                            requesterID);
                    }
                }
                else
                {
                    // SignalR Connection IDs might start with a hyphen.  We surround them
                    // with quotes so the command line will be parsed correctly.
                    Process.Start(_rcBinaryPath, $"-mode Unattended " +
                        $"-requester \"{requesterID}\" " +
                        $"-serviceid \"{serviceID}\" " +
                        $"-deviceid {ConnectionInfo.DeviceID} " +
                        $"-host {ConnectionInfo.Host} " +
                        $"-orgid \"{ConnectionInfo.OrganizationID}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
                await hubConnection.SendAsync("DisplayMessage",
                    "Nie udało się uruchomić kontroli zdalnej na urządzeniu docelowym.",
                    "Nie udało się uruchomić pilota.",
                    "bg-danger",
                    requesterID);
            }
        }
        public async Task RestartScreenCaster(List<string> viewerIDs, string serviceID, string requesterID, HubConnection hubConnection, int targetSessionID = -1)
        {
            try
            {
                // Start Desktop app.                 
                Logger.Write("Ponowne uruchamianie rzutnika ekranu.");
                if (WindowsIdentity.GetCurrent().IsSystem)
                {
                    // Give a little time for session changing, etc.
                    await Task.Delay(1000);

                    var result = Win32Interop.OpenInteractiveProcess(_rcBinaryPath + 
                            $" -mode Unattended" +
                            $" -requester \"{requesterID}\"" +
                            $" -serviceid \"{serviceID}\"" +
                            $" -deviceid {ConnectionInfo.DeviceID}" +
                            $" -host {ConnectionInfo.Host}" +
                            $" -orgid \"{ConnectionInfo.OrganizationID}\"" +
                            $" -relaunch true" +
                            $" -viewers {String.Join(",", viewerIDs)}",

                        targetSessionId: targetSessionID,
                        forceConsoleSession: Shlwapi.IsOS(OsType.OS_ANYSERVER) && targetSessionID == -1,
                        desktopName: "default",
                        hiddenWindow: true,
                        out _);

                    if (!result)
                    {
                        Logger.Write("Nie udało się ponownie uruchomić narzędzia do rzucania ekranu.");
                        await hubConnection.SendAsync("SendConnectionFailedToViewers", viewerIDs);
                        await hubConnection.SendAsync("DisplayMessage",
                            "Nie udało się uruchomić kontroli zdalnej na urządzeniu docelowym.",
                            "Nie udało się uruchomić pilota.",
                            "bg-danger",
                            requesterID);
                    }
                }
                else
                {
                    // SignalR Connection IDs might start with a hyphen.  We surround them
                    // with quotes so the command line will be parsed correctly.
                    Process.Start(_rcBinaryPath, 
                        $"-mode Unattended " +
                        $"-requester \"{requesterID}\" " +
                        $"-serviceid \"{serviceID}\" " +
                        $"-deviceid {ConnectionInfo.DeviceID} " +
                        $"-host {ConnectionInfo.Host} " +
                        $" -orgid \"{ConnectionInfo.OrganizationID}\"" +
                        $"-relaunch true " +
                        $"-viewers {String.Join(",", viewerIDs)}");
                }
            }
            catch (Exception ex)
            {
                await hubConnection.SendAsync("SendConnectionFailedToViewers", viewerIDs);
                Logger.Write(ex);
                throw;
            }
        }
    }
}
