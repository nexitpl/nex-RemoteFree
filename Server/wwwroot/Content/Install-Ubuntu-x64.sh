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
        rm -r -f /usr/local/bin/nex-Remote
        rm -f /etc/systemd/system/nex-Remote-agent.service
        systemctl daemon-reload
        exit
    elif [ "${Args[$i]}" = "--path" ]; then
        UpdatePackagePath="${Args[$i+1]}"
    fi
done

UbuntuVersion=$(lsb_release -r -s)

wget -q https://packages.microsoft.com/config/ubuntu/$UbuntuVersion/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update
apt-get -y install apt-transport-https
apt-get update
apt-get -y install dotnet-runtime-5.0
rm packages-microsoft-prod.deb

apt-get -y install libx11-dev
apt-get -y install libxrandr-dev
apt-get -y install unzip
apt-get -y install libc6-dev
apt-get -y install libgdiplus
apt-get -y install libxtst-dev
apt-get -y install xclip
apt-get -y install jq
apt-get -y install curl


if [ -f "/usr/local/bin/nex-Remote/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/nex-Remote/ConnectionInfo.json" | jq -r '.DeviceID'`
     if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /usr/local/bin/nex-Remote
rm -f /etc/systemd/system/nex-Remote-agent.service

mkdir -p /usr/local/bin/nex-Remote/
cd /usr/local/bin/nex-Remote/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Downloading client..." >> /tmp/nex-Remote_Install.log
    wget $HostName/Content/nex-Remote-Linux.zip
else
    echo  "Copying install files..." >> /tmp/nex-Remote_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/nex-Remote/nex-Remote-Linux.zip
    rm -f "$UpdatePackagePath"
fi

unzip ./nex-Remote-Linux.zip
rm -f ./nex-Remote-Linux.zip
chmod +x ./nex-Remote_Agent
chmod +x ./Desktop/nex-Remote_Desktop


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
systemctl restart nex-Remote-agent

echo Install complete. >> /tmp/nex-Remote_Install.log
