<#
.SYNOPSIS
   Installs the nex-RemoteFree Client.
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
$LogPath = "$env:TEMP\nex-RemoteFree_Install.txt"
[string]$HostName = $null
[string]$Organization = $null
$ConnectionInfo = $null

if ([System.Environment]::Is64BitOperatingSystem){
	$Platform = "x64"
}
else {
	$Platform = "x86"
}

$InstallPath = "$env:ProgramFiles\nex-RemoteFree"

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

function Stop-nex-RemoteFree {
	Start-Process -FilePath "cmd.exe" -ArgumentList "/c sc delete nex-RemoteFree_Service" -Wait -WindowStyle Hidden
	Stop-Process -Name nex-RemoteFree_Agent -Force -ErrorAction SilentlyContinue
	Stop-Process -Name nex-RemoteFree_Desktop -Force -ErrorAction SilentlyContinue
}

function Uninstall-nex-RemoteFree {
	Stop-nex-RemoteFree
	Remove-Item -Path $InstallPath -Force -Recurse -ErrorAction SilentlyContinue
	Remove-NetFirewallRule -Name "nex-RemoteFree ScreenCast" -ErrorAction SilentlyContinue
}

function Install-nex-RemoteFree {
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
		Copy-Item -Path $Path -Destination "$env:TEMP\nex-RemoteFree-Win10-$Platform.zip"

	}
	else {
		$ProgressPreference = 'SilentlyContinue'
		Write-Log "Pobieranie nex-RemoteFree..."
		Invoke-WebRequest -Uri "$HostName/Content/nex-RemoteFree-Win10-$Platform.zip" -OutFile "$env:TEMP\nex-RemoteFree-Win10-$Platform.zip" 
		$ProgressPreference = 'Continue'
	}

	if (!(Test-Path -Path "$env:TEMP\nex-RemoteFree-Win10-$Platform.zip")) {
		Write-Log "Nie udało się pobrać plików nex-RemoteFree."
		Do-Exit
	}

	Stop-nex-RemoteFree
	Get-ChildItem -Path "C:\Program Files\nex-RemoteFree" | Where-Object {$_.Name -notlike "ConnectionInfo.json"} | Remove-Item -Recurse -Force

	Expand-Archive -Path "$env:TEMP\nex-RemoteFree-Win10-$Platform.zip" -DestinationPath "$InstallPath"  -Force

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

	New-Service -Name "nex-RemoteFree_Service" -BinaryPathName "$InstallPath\nex-RemoteFree_Agent.exe" -DisplayName "nex-RemoteFree_Service" -StartupType Automatic -Description "Usługa działająca w tle, która utrzymuje połączenie z serwerem nex-RemoteFree.  Usługa służy do zdalnego wsparcia i konserwacji przez oprogramowanie nex-RemoteFree by nex-IT Jakub Potoczny."
	Start-Process -FilePath "cmd.exe" -ArgumentList "/c sc.exe failure `"nex-RemoteFree_Service`" reset=5 actions=restart/5000" -Wait -WindowStyle Hidden
	Start-Service -Name nex-RemoteFree_Service

	New-NetFirewallRule -Name "nex-RemoteFree Desktop Unattended" -DisplayName "nex-RemoteFree Desktop Unattended" -Description "Agent, który umożliwia udostępnianie ekranu i zdalne sterowanie dla nex-RemoteFree." -Direction Inbound -Enabled True -Action Allow -Program "C:\Program Files\nex-RemoteFree\Desktop\nex-RemoteFree_Desktop.exe" -ErrorAction SilentlyContinue
}

try {
	Run-StartupChecks

	Write-Log "Dzienniki instalacji/odinstalowania są zapisywane do `"$LogPath`""
    Write-Log

	if ($Uninstall) {
		Write-Log "Rozpoczęto dezinstalację."
		Uninstall-nex-RemoteFree
		Write-Log "Dezinstalacja zakończona."
		exit
	}
	else {
		Write-Log "Instalacja rozpoczęta."
        Write-Log
		Install-nex-RemoteFree
		Write-Log "Instalacja zakończona."
		exit
	}
}
catch {
	Write-Log -Message "Wystąpił błąd: $($Error[0].InvocationInfo.PositionMessage)"
	Do-Exit
}
