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
  mkdir -p /var/www/nex-RemoteFree && \
  mkdir /config && \
  wget -q https://github.com/nexitpl/nex-RemoteFree/releases/latest/download/nex-RemoteFree_Server_Linux-x64.zip && \
  unzip -o nex-RemoteFree_Server_Linux-x64.zip -d /var/www/nex-RemoteFree && \
  rm nex-RemoteFree_Server_Linux-x64.zip

RUN \
  mkdir -p /nexRemoteFree-data && \
  sed -i 's/DataSource=nexRemoteFree.db/DataSource=\/nexRemoteFree-data\/nexRemoteFree.db/' /var/www/nex-RemoteFree/appsettings.json

VOLUME "/nexRemoteFree-data"

WORKDIR /var/www/nex-RemoteFree

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]