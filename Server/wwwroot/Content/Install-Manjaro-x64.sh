#!/bin/bash
HostName=
Organization=
GUID=$(cat /proc/sys/kernel/random/uuid)
UpdatePackagePath=""


Args=( "$@" )
ArgLength=${#Args[@]}

for (( i=0; i<${ArgLength}; i+=2 ));
do
    if [ "${Args[$i]}" = "--uninstall" ]; then
        systemctl stop nex-Remote-agent
        rm -r -f /usr/local/bin/Remotely
        rm -f /etc/systemd/system/nex-Remote-agent.service
        systemctl daemon-reload
        exit
    elif [ "${Args[$i]}" = "--path" ]; then
        UpdatePackagePath="${Args[$i+1}"
    fi
done

pacman -Sy
pacman -S dotnet-runtime-5.0 --noconfirm
pacman -S libx11 --noconfirm
pacman -S unzip --noconfirm
pacman -S libc6 --noconfirm
pacman -S libgdiplus --noconfirm
pacman -S libxtst --noconfirm
pacman -S xclip --noconfirm
pacman -S jq --noconfirm
pacman -S curl --noconfirm

if [ -f "/usr/local/bin/nex-Remote/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/nex-Remote/ConnectionInfo.json" | jq -r '.DeviceID'`
    if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /usr/local/bin/nex-Remote
rm -f /etc/systemd/system/remotely-agent.service

mkdir -p /usr/local/bin/nex-Remote/
cd /usr/local/bin/nex-Remote/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Pobieranie Klienta..." >> /tmp/nex-Remote_Install.log
    wget $HostName/Content/nex-Remote-Linux.zip
else
    echo  "Kopiowanie plików..." >> /tmp/nex-Remote_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/nex-Remote/nex-Remote-Linux.zip
    rm -f "$UpdatePackagePath"
fi

unzip ./nex-Remote-Linux.zip
rm -f ./nex-Remote-Linux.zip
chmod +x ./Remotely_Agent
chmod +x ./Desktop/Remotely_Desktop


connectionInfo="{
    \"DeviceID\":\"$GUID\", 
    \"Host\":\"$HostName\",
    \"OrganizationID\": \"$Organization\",
    \"ServerVerificationToken\":\"\"
}"

echo "$connectionInfo" > ./ConnectionInfo.json

curl --head $HostName/Content/nex-Remote-Linux.zip | grep -i "etag" | cut -d' ' -f 2 > ./etag.txt

echo Creating service... >> /tmp/nex-Remote_Install.log

serviceConfig="[Unit]
Description=The nex-Remote agent used for remote access.

[Service]
WorkingDirectory=/usr/local/bin/nex-Remote/
ExecStart=/usr/local/bin/nex-Remote/nex-Remote_Agent
Restart=always
StartLimitIntervalSec=0
RestartSec=10

[Install]
WantedBy=graphical.target"

echo "$serviceConfig" > /etc/systemd/system/nex-Remote-agent.service

systemctl enable nex-Remote-agent
systemctl restart remotnex-Remoteely-agent

echo Install complete. >> /tmp/nex-Remote_Install.log