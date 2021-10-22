FROM ubuntu:focal

EXPOSE 5002

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5002"

RUN \
  apt-get -y update && \
  apt-get -y install \
  apt-utils \
  wget \
  apt-transport-https \
  unzip \
  acl \
  libssl1.0

RUN \
  wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
  dpkg -i packages-microsoft-prod.deb && \
  apt-get -y update && \
  apt-get -y install aspnetcore-runtime-5.0

RUN \
  mkdir -p /var/www/nex-Remote && \
  mkdir /config && \
  wget -q https://github.com/lucent-sea/Remotely/releases/latest/download/nex-Remote_Server_Linux-x64.zip && \
  unzip -o nex-Remote_Server_Linux-x64.zip -d /var/www/nex-Remote && \
  rm nex-Remote_Server_Linux-x64.zip

RUN \
  mkdir -p /remotely-data && \
  sed -i 's/DataSource=Remotely.db/DataSource=\/remotely-data\/Remotely.db/' /var/www/nex-Remote/appsettings.json

VOLUME "/remotely-data"

WORKDIR /var/www/nex-Remote

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]