#!/bin/bash

echo "Entered main script."

ServerDir=/var/www/nex-Remote
RemotelyData=/remotely-data

AppSettingsVolume=/remotely-data/appsettings.json
AppSettingsWww=/var/www/nex-Remote/appsettings.json

if [ ! -f "$AppSettingsVolume" ]; then
	echo "Copying appsettings.json to volume."
	cp "$AppSettingsWww" "$AppSettingsVolume"
fi

if [ -f "$AppSettingsWww" ]; then
	rm "$AppSettingsWww"
fi

ln -s "$AppSettingsVolume" "$AppSettingsWww"

echo "Starting nex-Remote server."
exec /usr/bin/dotnet /var/www/nex-Remote/nex-Remote_Server.dll