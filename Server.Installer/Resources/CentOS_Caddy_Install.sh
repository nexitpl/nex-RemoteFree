#!/bin/bash
echo "Thanks for trying nex-RemoteFree!"
echo


Args=( "$@" )
ArgLength=${#Args[@]}

for (( i=0; i<${ArgLength}; i+=2 ));
do
    if [ "${Args[$i]}" = "--host" ]; then
        HostName="${Args[$i+1]}"
    elif [ "${Args[$i]}" = "--approot" ]; then
        AppRoot="${Args[$i+1]}"
    fi
done

if [ -z "$AppRoot" ]; then
    read -p "Enter path where the nex-RemoteFree server files should be installed (typically /var/www/nex-RemoteFree): " AppRoot
    if [ -z "$AppRoot" ]; then
        AppRoot="/var/www/nex-RemoteFree"
    fi
fi

if [ -z "$HostName" ]; then
    read -p "Enter server host (e.g. https://remote.nex-it.pl): " HostName
fi

chmod +x "$AppRoot/nex-RemoteFree_Server"

echo "Using $AppRoot as the nex-RemoteFree website's content directory."

yum update
yum -y install curl
yum -y install software-properties-common
yum -y install gnupg

# Install .NET Core Runtime.
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm

yum -y install apt-transport-https
yum -y update
yum -y install aspnetcore-runtime-5.0


 # Install other prerequisites.
yum -y install https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm
yum -y install yum-utils
yum-config-manager --enable rhui-REGION-rhel-server-extras rhui-REGION-rhel-server-optional
yum -y install unzip
yum -y install acl
yum -y install libc6-dev
yum -y install libgdiplus


# Install Caddy
yum install yum-plugin-copr
yum copr enable @caddy/caddy
yum install caddy


# Configure Caddy
caddyConfig="
$HostName {
    reverse_proxy 127.0.0.1:5002
}
"

echo "$caddyConfig" > /etc/caddy/Caddyfile


# Create service.

serviceConfig="[Unit]
Description=nex-RemoteFree Server

[Service]
WorkingDirectory=$AppRoot
ExecStart=/usr/bin/dotnet $AppRoot/nex-RemoteFree_Server.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
SyslogIdentifier=nexRemoteFree
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target"

echo "$serviceConfig" > /etc/systemd/system/nex-RemoteFree.service


# Enable service.
systemctl enable nex-RemoteFree.service
# Start service.
systemctl start nex-RemoteFree.service

firewall-cmd --permanent --zone=public --add-service=http
firewall-cmd --permanent --zone=public --add-service=https
firewall-cmd --reload

# Restart caddy
systemctl restart caddy