<#
.SYNOPSIS
   Installs the nex-Remote Client.
.DESCRIPTION
   Do not modify this script.  It was generated specifically for your account.
.EXAMPLE
   powershell.exe -f Install-Win10.ps1
   powershell.exe -f Install-Win10.ps1 -DeviceAlias "My Super Computer" -DeviceGroup "My Stuff"
#>

param (
	[string]$DeviceAlias,
	[string]$DeviceGroup,
	[string]$Path,
	[switch]$Uninstall
)

[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
$LogPath = "$env:TEMP\nex-Remote_Install.txt"
[string]$HostName = $null
[string]$Organization = $null
$ConnectionInfo = $null

if ([System.Environment]::Is64BitOperatingSystem){
	$Platform = "x64"
}
else {
	$Platform = "x86"
}

$InstallPath = "$env:ProgramFiles\nex-Remote"

function Write-Log($Message){
	Write-Host $Message
	"$((Get-Date).ToString()) - $Message" | Out-File -FilePath $LogPath -Append
}
function Do-Exit(){
	Write-Host "Kończenie..."
	Start-Sleep -Seconds 3
	exit
}
function Is-Administrator() {
    $Identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $Principal = New-Object System.Security.Principal.WindowsPrincipal -ArgumentList $Identity
    return $Principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
} 

function Run-StartupChecks {

	if ($HostName -eq $null -or $Organization -eq $null) {
		Write-Log "Brak wymaganych parametrów.  Spróbuj ponownie pobrać instalator."
		Do-Exit
	}

	if ((Is-Administrator) -eq $false) {
		Write-Log -Message "Skrypt instalacyjny wymaga podwyższenia poziomu uprawnień.  Próba podniesienia uprawnień..."
		Start-Sleep -Seconds 3
		$param = "-f `"$($MyInvocation.ScriptName)`""

		Start-Process -FilePath powershell.exe -ArgumentList "-DeviceAlias $DeviceAlias -DeviceGroup $DeviceGroup -Path $Path" -Verb RunAs
		exit
	}
}

function Stop-Remotely {
	Start-Process -FilePath "cmd.exe" -ArgumentList "/c sc delete nex-Remote_Service" -Wait -WindowStyle Hidden
	Stop-Process -Name Remotely_Agent -Force -ErrorAction SilentlyContinue
	Stop-Process -Name Remotely_Desktop -Force -ErrorAction SilentlyContinue
}

function Uninstall-Remotely {
	Stop-Remotely
	Remove-Item -Path $InstallPath -Force -Recurse -ErrorAction SilentlyContinue
	Remove-NetFirewallRule -Name "nex-Remote ScreenCast" -ErrorAction SilentlyContinue
}

function Install-Remotely {
	if ((Test-Path -Path "$InstallPath") -and (Test-Path -Path "$InstallPath\ConnectionInfo.json")) {
		$ConnectionInfo = Get-Content -Path "$InstallPath\ConnectionInfo.json" | ConvertFrom-Json
		if ($ConnectionInfo -ne $null) {
			$ConnectionInfo.Host = $HostName
			$ConnectionInfo.OrganizationID = $Organization
			$ConnectionInfo.ServerVerificationToken = ""
		}
	}
	else {
		New-Item -ItemType Directory -Path "$InstallPath" -Force
	}

	if ($ConnectionInfo -eq $null) {
		$ConnectionInfo = @{
			DeviceID = (New-Guid).ToString();
			Host = $HostName;
			OrganizationID = $Organization;
			ServerVerificationToken = "";
		}
	}

	if ($HostName.EndsWith("/")) {
		$HostName = $HostName.Substring(0, $HostName.LastIndexOf("/"))
	}

	if ($Path) {
		Write-Log "Kopiowanie plików instalacyjnych..."
		Copy-Item -Path $Path -Destination "$env:TEMP\nex-Remote-Win10-$Platform.zip"

	}
	else {
		$ProgressPreference = 'SilentlyContinue'
		Write-Log "Pobieranie nex-Remote..."
		Invoke-WebRequest -Uri "$HostName/Content/nex-Remote-Win10-$Platform.zip" -OutFile "$env:TEMP\nex-Remote-Win10-$Platform.zip" 
		$ProgressPreference = 'Continue'
	}

	if (!(Test-Path -Path "$env:TEMP\nex-Remote-Win10-$Platform.zip")) {
		Write-Log "Nie udało się pobrać plików nex-Remote."
		Do-Exit
	}

	Stop-Remotely
	Get-ChildItem -Path "C:\Program Files\nex-Remote" | Where-Object {$_.Name -notlike "ConnectionInfo.json"} | Remove-Item -Recurse -Force

	Expand-Archive -Path "$env:TEMP\nex-Remote-Win10-$Platform.zip" -DestinationPath "$InstallPath"  -Force

	New-Item -ItemType File -Path "$InstallPath\ConnectionInfo.json" -Value (ConvertTo-Json -InputObject $ConnectionInfo) -Force

	if ($DeviceAlias -or $DeviceGroup) {
		$DeviceSetupOptions = @{
			DeviceAlias = $DeviceAlias;
			DeviceGroup = $DeviceGroup;
			OrganizationID = $Organization;
			DeviceID = $ConnectionInfo.DeviceID;
		}

		Invoke-RestMethod -Method Post -ContentType "application/json" -Uri "$HostName/api/devices" -Body $DeviceSetupOptions -UseBasicParsing
	}

	New-Service -Name "nex-Remote_Service" -BinaryPathName "$InstallPath\nex-Remote_Agent.exe" -DisplayName "nex-Remote_Service" -StartupType Automatic -Description "Usługa działająca w tle, która utrzymuje połączenie z serwerem nex-Remote.  Usługa służy do zdalnego wsparcia i konserwacji przez oprogramowanie nex-Remote by nex-IT Jakub Potoczny."
	Start-Process -FilePath "cmd.exe" -ArgumentList "/c sc.exe failure `"Remotely_Service`" reset=5 actions=restart/5000" -Wait -WindowStyle Hidden
	Start-Service -Name Remotely_Service

	New-NetFirewallRule -Name "nex-Remote Desktop Unattended" -DisplayName "nex-Remote Desktop Unattended" -Description "Agent, który umożliwia udostępnianie ekranu i zdalne sterowanie dla nex-Remote." -Direction Inbound -Enabled True -Action Allow -Program "C:\Program Files\nex-Remote\Desktop\nex-Remote_Desktop.exe" -ErrorAction SilentlyContinue
}

try {
	Run-StartupChecks

	Write-Log "Dzienniki instalacji/odinstalowania są zapisywane do `"$LogPath`""
    Write-Log

	if ($Uninstall) {
		Write-Log "Rozpoczęto dezinstalację."
		Uninstall-Remotely
		Write-Log "Dezinstalacja zakończona."
		exit
	}
	else {
		Write-Log "Instalacja rozpoczęta."
        Write-Log
		Install-Remotely
		Write-Log "Instalacja zakończona."
		exit
	}
}
catch {
	Write-Log -Message "Wystąpił błąd: $($Error[0].InvocationInfo.PositionMessage)"
	Do-Exit
}
