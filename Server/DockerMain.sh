#!/bin/bash

echo "Entered main script."

ServerDir=/var/www/nex-RemoteFree
nexRemoteFreeData=/nexRemoteFree-data

AppSettingsVolume=/nexRemoteFree-data/appsettings.json
AppSettingsWww=/var/www/nex-RemoteFree/appsettings.json

if [ ! -f "$AppSettingsVolume" ]; then
	echo "Copying appsettings.json to volume."
	cp "$AppSettingsWww" "$AppSettingsVolume"
fi

if [ -f "$AppSettingsWww" ]; then
	rm "$AppSettingsWww"
fi

ln -s "$AppSettingsVolume" "$AppSettingsWww"

echo "Starting nex-RemoteFree server."
exec /usr/bin/dotnet /var/www/nex-RemoteFree/nex-RemoteFree_Server.dll