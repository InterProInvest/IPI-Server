1. System update and installation of necessary packages 
 
 CentOS 8/RHEL 8 Server
 ```shell
 # dnf update -y
 # dnf install git -y
 ```
Ubuntu 18.04
 ```shell
 # apt update
 # apt upgrade -y
 # apt install apt-transport-https ca-certificates curl software-properties-common -y
 ```
2. Enable Docker CE Repository 

CentOS 8/RHEL 8 Server
```shell
 # dnf config-manager --add-repo=https://download.docker.com/linux/centos/docker-ce.repo
 ```
Ubuntu 18.04  
```shell
# curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
# add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu bionic stable"
# apt update
``` 
3. Install Docker CE

CentOS 8/RHEL 8 Server
```shell
# dnf install docker-ce --nobest -y
# systemctl start docker
# systemctl enable docker
```
Ubuntu 18.04 
```shell
# apt-cache policy docker-ce
# apt install docker-ce -y
``` 
Run the following command to verify installed docker version
 ```shell
$ docker --version
```
```shell
Docker version 19.03.8, build afacb8b
```
4. Install Docker Compose
```shell
# curl -L https://github.com/docker/compose/releases/download/1.25.4/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
# chmod +x /usr/local/bin/docker-compose
```
Note: Replace “1.25.4” with docker compose version that you want to install but at this point of time this is the latest and stable version of docker compos

5. Clone repository
```shell
git clone https://github.com/HideezGroup/HES.git
cd HES/HES.Docker
```
6. Installation of the project:
these commands should be executed in the directory where the main files are: Dockerfile, docker-compose.yml, appsettings.json
```shell
# docker-compose up -d --build && docker-compose down &&docker-compose up -d && docker-compose ps
```
