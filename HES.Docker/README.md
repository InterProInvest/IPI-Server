
# 1. System update and installation of necessary packages 
  CentOS 7
 ```shell
 # yum update -y
 # yum install git -y
 ```
Ubuntu 18.04
 ```shell
 # apt update
 # apt upgrade -y
 # apt install apt-transport-https ca-certificates curl software-properties-common -y
 ```
# 2. Enable Docker CE Repository 

CentOS 7
```shell
 # yum install -y yum-utils device-mapper-persistent-data lvm2
 # yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
 ```
Ubuntu 18.04  
```shell
# curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
# add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu bionic stable"
# apt update
``` 
# 3. Install Docker CE

CentOS 7
```shell
# yum install docker-ce -y
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
# 4. Install Docker Compose
```shell
# curl -L https://github.com/docker/compose/releases/download/1.25.4/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
# chmod +x /usr/local/bin/docker-compose
```
Note: Replace “1.25.4” with docker compose version that you want to install but at this point of time this is the latest and stable version of docker compos

5. Clone repository
```shell
# git clone https://github.com/HideezGroup/HES.git /opt/src/HES
$ cd HES/HES.Docker
```

# 6. Create folders for HES and copy appsettings.json
```shel
# mkdir /opt/HES
# cp -r /opt/src/HES/HES.Docker/* /opt/HES
# mkdir /opt/HES/<Name_Of_Domain>
# mkdir /opt/HES/<Name_Of_Domain>/logs
# cp /opt/src/HES/HES.Web/appsettings.json /opt/HES/<Name_Of_Domain>
```

# 7. Setting

Edit `/opt/HES/<Name_Of_Domain>/appsettings.json`  
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=hes-db;port=3306;database=db;uid=user;pwd=password"
  },
  "EmailSender": {
    "Host": "smtp.example.com",
    "Port": 123,
    "EnableSSL": true,
    "UserName": "user@example.com",
    "Password": "password"
  },
  "DataProtection": {
    "Password": "password"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```
* **mysql_server** - MySQL server ip address (example `127.0.0.1`)
* **mysql_port** - MySQL server port (example `3306`)
* **your_db** - The name of your database on the MySQL server (example `hes`)
* **your_user** - MySQL database username (example `admin`)
* **your_secret** - Password from database user on MySQL server (example `password`)
* **email_host** - Host your email server (example `smtp.example.com`)
* **email_port** - Port your email server (example `123`)
* **your_email_name** - Your email name (example `user@example.com`)
* **your_email_password** - Your email name (example `password`)
* **protection_password** - Your password for database encryption (example `password`)

**[Note]** 
parameter **server** must be the name of the container on which MySQL will run (hes-db)  in `/opt/HES/docker-compose.yml` 
the parameters  **database,  uid** and **pwd** should be the same as in the file `/opt/HES/docker-compose.yml`  (**MYSQL_DATABASE, MYSQL_USER,  MYSQL_PASSWORD**
You can change them at this point     

edit file
`/opt/HES/docker-compose.yml`
replace <Name_Of_Domain> with you correct domain

```yml
version: '3.6'
# Create networks
networks:
  hes:
    name: hes
# Create Sevices
services:
  # Create HES images
  hes:
    container_name: hes-<Name_Of_Domain>
    command: ./HES.Web
    build: .
    environment: 
      ASPNETCORE_URLS: "http://0.0.0.0:5000;https://0.0.0.0:5001"
    ports:
      - "5000:5000"
      - "5001:5001"
    networks:
      - hes
    depends_on:
      - hes-db
    volumes:
      - ./<Name_Of_Domain>/appsettings.json:/opt/HES/appsettings.json
      - ./<Name_Of_Domain>/logs:/opt/HES/logs      
  hes-db:
    image: mysql
    container_name: hes-db
    command: --default-authentication-plugin=mysql_native_password
    environment:
      # these variables specify the name of the database and passwords 
      # (the same database, user and password must be written in the appsettings.json file)
      MYSQL_DATABASE: db
      MYSQL_USER: user
      MYSQL_PASSWORD: password
      MYSQL_ROOT_PASSWORD: password
    ports:
      - '3306:3306'
    networks: 
      - hes
    volumes:
      #./mysql/data is a real folder on the local machine that will host the MySQL databaseґ
      # you can change it to a more appropriate one 
      - ./mysql/data:/var/lib/mysql
  hes-nginx:
    image: nginx
    container_name: hes-nginx
    ports:
      - '80:80'
      - '443:443'
    networks:
      - hes
    depends_on:
      - hes
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/certs:/etc/nginx/certs
```
and finally, make the changes to the file
` /opt/HES/nginx/nginx.conf`
correct  <Name_Of_Domain> with you damain:
```conf
user  nginx;
worker_processes  1;
error_log  /var/log/nginx/error.log warn;
pid        /var/run/nginx.pid;
events {
    worker_connections  1024;
}
http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;
    log_format  main  '$remote_addr - $remote_user [$time_local] "$request" '
                      '$status $body_bytes_sent "$http_referer" '
                      '"$http_user_agent" "$http_x_forwarded_for"';
    access_log  /var/log/nginx/access.log  main;
    sendfile        on;
    #tcp_nopush     on;
    keepalive_timeout  65;
    #gzip  on;
    include /etc/nginx/conf.d/*.conf;
    map $http_upgrade $connection_upgrade {
                default Upgrade;
                ''      close;
    }

 # redirect all traffic to https
 server {
          server_name <Name_Of_Domain>;
          listen 80;
          listen [::]:80;
          if ($host = <Name_Of_Domain>) {
                return 301 https://$host$request_uri;
          }
          return 404;
  }

  server {
          server_name <Name_Of_Domain>;
          listen [::]:443 ssl ;
          listen 443 ssl;
          ssl_certificate "certs/<Name_Of_Domain>.crt";
          ssl_certificate_key "certs/<Name_Of_Domain>.key";

          location / {
                 proxy_pass https://hes-<Name_Of_Domain>:5001;
                 proxy_http_version 1.1;
                 proxy_set_header Upgrade $http_upgrade;
                 proxy_set_header  Connection $connection_upgrade;
                 proxy_set_header Host $host;
                 proxy_cache_bypass $http_upgrade;
                 proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                 proxy_set_header X-Forwarded-Proto $scheme;
          }
    }
}
```

 Creating a Self-Signed SSL Certificate for Nginx  Note For a "real" site, you should take care of acquiring a certificate from a certificate authority.  For self-signed certificate, browser will alert you that site has security issues. Replace <Name_Of_Domain> with you domain name (when generating a certificate, answer a few simple questions)
 
```shel
# sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /opt/HES/nginx/certs/<Name_Of_Domain>.key -out /opt/HES/nginx/certs/<Name_Of_Domain>.crt
```


# 8. Installation of the project:
these commands should be executed in the directory where the main files are: Dockerfile, docker-compose.yml 
Build container:
```shell
# cd /opt/HES
# docker-compose up -d --build
```
Restart  containers:
```shell
# docker-compose down && docker-compose up -d
```

Check status:
```shell
# docker-compose ps
```

