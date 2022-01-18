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
        systemctl stop nex-RemoteFree-agent
        rm -r -f /usr/local/bin/nex-RemoteFree
        rm -f /etc/systemd/system/nex-RemoteFree-agent.service
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

if [ -f "/usr/local/bin/nex-RemoteFree/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/nex-RemoteFree/ConnectionInfo.json" | jq -r '.DeviceID'`
    if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /usr/local/bin/nex-RemoteFree
rm -f /etc/systemd/system/nex-RemoteFree-agent.service

mkdir -p /usr/local/bin/nex-RemoteFree/
cd /usr/local/bin/nex-RemoteFree/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Pobieranie Klienta..." >> /tmp/nex-RemoteFree_Install.log
    wget $HostName/Content/nex-RemoteFree-Linux.zip
else
    echo  "Kopiowanie plików..." >> /tmp/nex-RemoteFree_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/nex-RemoteFree/nex-RemoteFree-Linux.zip
    rm -f "$UpdatePackagePath"
fi

unzip ./nex-RemoteFree-Linux.zip
rm -f ./nex-RemoteFree-Linux.zip
chmod +x ./nex-RemoteFree_Agent
chmod +x ./Desktop/nex-RemoteFree_Desktop


connectionInfo="{
    \"DeviceID\":\"$GUID\", 
    \"Host\":\"$HostName\",
    \"OrganizationID\": \"$Organization\",
    \"ServerVerificationToken\":\"\"
}"

echo "$connectionInfo" > ./ConnectionInfo.json

curl --head $HostName/Content/nex-RemoteFree-Linux.zip | grep -i "etag" | cut -d' ' -f 2 > ./etag.txt

echo Creating service... >> /tmp/nex-RemoteFree_Install.log

serviceConfig="[Unit]
Description=The nex-RemoteFree agent used for remote access.

[Service]
WorkingDirectory=/usr/local/bin/nex-RemoteFree/
ExecStart=/usr/local/bin/nex-RemoteFree/nex-RemoteFree_Agent
Restart=always
StartLimitIntervalSec=0
RestartSec=10

[Install]
WantedBy=graphical.target"

echo "$serviceConfig" > /etc/systemd/system/nex-RemoteFree-agent.service

systemctl enable nex-RemoteFree-agent
systemctl restart remotnex-RemoteFreeely-agent

echo Install complete. >> /tmp/nex-RemoteFree_Install.log