#!/bin/bash

HostName=
Organization=
GUID="$(uuidgen)"
UpdatePackagePath=""

Args=( "$@" )
ArgLength=${#Args[@]}

for (( i=0; i<${ArgLength}; i+=2 ));
do
    if [ "${Args[$i]}" = "--uninstall" ]; then
        launchctl unload -w /Library/LaunchDaemons/nex-RemoteFree-agent.plist
        rm -r -f /usr/local/bin/nex-RemoteFree/
        rm -f /Library/LaunchDaemons/nex-RemoteFree-agent.plist
        exit
    elif [ "${Args[$i]}" = "--path" ]; then
        UpdatePackagePath="${Args[$i+1]}"
    fi
done


# Install Homebrew
su - $SUDO_USER -c '/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"'
su - $SUDO_USER -c "brew update"

# Install .NET Runtime
su - $SUDO_USER -c "brew install --cask dotnet"

# Install dependency for System.Drawing.Common
su - $SUDO_USER -c "brew install mono-libgdiplus"

# Install other dependencies
su - $SUDO_USER -c "brew install curl"
su - $SUDO_USER -c "brew install jq"


if [ -f "/usr/local/bin/nex-RemoteFree/ConnectionInfo.json" ]; then
    SavedGUID=`cat "/usr/local/bin/nex-RemoteFree/ConnectionInfo.json" | jq -r '.DeviceID'`
    if [[ "$SavedGUID" != "null" && -n "$SavedGUID" ]]; then
        GUID="$SavedGUID"
    fi
fi

rm -r -f /Applications/nex-RemoteFree
rm -f /Library/LaunchDaemons/nex-RemoteFree-agent.plist

mkdir -p /usr/local/bin/nex-RemoteFree/
chmod -R 755 /usr/local/bin/nex-RemoteFree/
cd /usr/local/bin/nex-RemoteFree/

if [ -z "$UpdatePackagePath" ]; then
    echo  "Pobieranie klienta..." >> /tmp/nex-RemoteFree_Install.log
    curl $HostName/Content/nex-RemoteFree-MacOS-arm64.zip --output /usr/local/bin/nex-RemoteFree/nex-RemoteFree-MacOS-arm64.zip
else
    echo  "Kopiowanie plików instalacyjnych..." >> /tmp/nex-RemoteFree_Install.log
    cp "$UpdatePackagePath" /usr/local/bin/nex-RemoteFree/nex-RemoteFree-MacOS-arm64.zip
    rm -f "$UpdatePackagePath"
fi

unzip -o ./nex-RemoteFree-MacOS-arm64.zip
rm -f ./nex-RemoteFree-MacOS-arm64.zip


connectionInfo="{
    \"DeviceID\":\"$GUID\", 
    \"Host\":\"$HostName\",
    \"OrganizationID\": \"$Organization\",
    \"ServerVerificationToken\":\"\"
}"

echo "$connectionInfo" > ./ConnectionInfo.json

curl --head $HostName/Content/nex-RemoteFree-MacOS-arm64.zip | grep -i "etag" | cut -d' ' -f 2 > ./etag.txt


plistFile="<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">
<plist version=\"1.0\">
<dict>
    <key>Label</key>
    <string>com.nexit.nex-RemoteFree-agent</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/dotnet</string>
        <string>/usr/local/bin/nex-RemoteFree/nex-RemoteFree_Agent.dll</string>
    </array>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>"
echo "$plistFile" > "/Library/LaunchDaemons/nex-RemoteFree-agent.plist"

launchctl load -w /Library/LaunchDaemons/nex-RemoteFree-agent.plist
launchctl kickstart -k system/com.nexit.nex-RemoteFree-agent