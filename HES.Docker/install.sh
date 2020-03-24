#/bin/bash


system=$(uname -a)
     
sudo apt-get -y update
sudo apt-get -y install apt-transport-https ca-certificates curl software-properties-common && sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add - 
add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu bionic stable" 
sudo apt-get -y update
sudo apt-cache policy docker-ce
apt-get -y  install docker-ce
sudo curl -L "https://github.com/docker/compose/releases/download/1.25.4/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
docker-compose up -d --build && docker-compose down &&docker-compose up -d && docker-compose ps
     
docker_v=$(docker --version)
docker_compose_v=$(docker-compose --version)
echo $docker_v
echo $docker_compose_v
echo $system


