# Deployment on CentOS or Ubuntu Server

## Important Notice
Server installation consists of two parts:
The first part describes the general requirements.
The second part describes the installation already for a specific site, there may be several virtual sites, so this step can be repeated several times

## System Requirements

  * Can be installed on a bare metal or virtual server
  * 8GB drive
  * 2GB RAM
  * Option 1: Clean installation of CentOS Linux x86_64 (tested on version 7.6), select "minimal install" option during installation
  * Option 2: Clean installation of Ubuntu Server LTS 18.04
  
# 1. Preparation (Run once)
  
## 1.1 System Update
  
  (if not yet updated)

*CentOS*
```shell
  $ sudo yum update -y
  $ sudo reboot
```
*Ubuntu*
```shell
  $ sudo apt update
  $ sudo apt upgrade -y
  $ sudo reboot
```

## 1.2 Disable SELinux (CentOS only)

```shell
  $ sudo sed 's/SELINUX=enforcing/SELINUX=disabled/' /etc/sysconfig/selinux
  $ sudo setenforce 0
```

## 1.3 Install git

*CentOS*
```shell
  $ sudo yum install git -y
```
*Ubuntu*
```shell
  $ sudo apt install git -y
 ```

## 1.4 Download HES repository from GitHub

```shell
  $ sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
  $ sudo yum install dotnet-sdk-2.2
```

## 1.5 Add Microsoft Package Repository and install .NET Core

```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server
```

  ## Getting Started (fresh install)

### 1. Postinstalling and Securing MySQL Server

```shell
  $ dotnet --version
3.1.200
```

  It is recommended to say yes to the following questions:

```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server -y
```
*Ubuntu*
```shell
  $ wget -c https://dev.mysql.com/get/mysql-apt-config_0.8.14-1_all.deb
  $ sudo dpkg -i mysql-apt-config_0.8.14-1_all.deb
  $ sudo apt update
  $ sudo apt install mysql-server -y
```

Enable and start MySQL service:

*CentOS*
```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```
*Ubuntu*
```shell
  $ sudo systemctl restart mysql.service
  $ sudo systemctl enable mysql.service
```

After installing MySQL, if everything went well, you can check the version of the program

```shell
  $ mysql -V
mysql  Ver 8.0.17 for Linux on x86_64 (Source Distribution)
```

**Setting a permanent real root password and MySQL security settings**

MySQL expects that your new password should consist of at least 8 characters, contain uppercase and lowercase letters, numbers and special characters (do not forget the password you set, it will come in handy later). After a successful password change, the following questions are recommended to answer "Y":

```shell
  $ sudo mysql_secure_installation
```

```shell
  Please set a new password for user root:
  Re-enter new password:

  Do you wish to continue with the password provided?(Press y|Y for Yes, any other key for No) : Y

  Remove anonymous users? (Press y|Y for Yes, any other key for No) : Y

  Disallow root login remotely? (Press y|Y for Yes, any other key for No) : Y

  Remove test database and access to it? (Press y|Y for Yes, any other key for No) : Y

  Reload privilege tables now? (Press y|Y for Yes, any other key for No) : Y
```


To verify that everything is correct, you can run
```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```
After entering password, you will see MySQL console with a prompt:
```shell
  Enter password: 
Welcome to the MySQL monitor.  Commands end with ; or \g.
Your MySQL connection id is 13
Server version: 8.0.19 MySQL Community Server - GPL

### 2. Creating MySQL User and Database for the Hideez Enterprise Server

Oracle is a registered trademark of Oracle Corporation and/or its
affiliates. Other names may be trademarks of their respective
owners.

Type 'help;' or '\h' for help. Type '\c' to clear the current input statement.

mysql>
```

To exit from mySql console, press Ctrl+D.

## 1.7 Install Nginx

*CentOS 7*
```shell
  $ sudo yum install epel-release -y
  $ sudo yum install nginx -y
  $ sudo systemctl enable nginx
  $ systemctl restart nginx
```
*Ubuntu*
```shell
  $ sudo apt install nginx -y
  $ sudo systemctl enable nginx
  $ systemctl restart nginx
```

Check that nginx service is installed and started:
```shell
  $ sudo systemctl status nginx
```
  The output would be something like this:
 
```shell
* nginx.service - The nginx HTTP and reverse proxy server
   Loaded: loaded (/usr/lib/systemd/system/nginx.service; enabled; vendor preset: disabled)
   Active: active (running) since Sat 2020-01-25 08:22:56 UTC; 8min ago
  Process: 1702 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
  Process: 1700 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
  Process: 1699 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
 Main PID: 1704 (nginx)
   CGroup: /system.slice/nginx.service
           +-1704 nginx: master process /usr/sbin/nginx
           +-1705 nginx: worker process
 ```

After performing these steps, the server should already be accessible from the network and respond in the browser to the ip address or its domain name. (http://<ip_or_domain_name>)

Now the preparation is complete.

# 2 Installing the HES server (can be repeated for each new virtual domain)

## 2.1 Creating a MySQL user and database for Hideez Enterprise Server

** Starting the MySQL Server Console **

```shell
  mysql -h localhost -u root -p
```

```sql
  ### CREATE DATABASE
  mysql> CREATE DATABASE <your_db>;

  ### CREATE USER ACCOUNT
  mysql> CREATE USER '<your_user>'@'127.0.0.1' IDENTIFIED BY '<your_secret>';

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON <your_db>.* TO '<your_user>'@'127.0.0.1';
 
  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES;
```

You should remember database name, username and password, they will come in handy later.

## 2.2 Installing Hideez Enterprise Server from source

Instead of <Name_Of_Domain>, specify the name of your site

```shell
  $ sudo yum install git && cd /opt
  $ sudo git clone https://github.com/HideezGroup/HES src && cd src/HES.Web/
```

here is an example for the case when our site will be called hideez.example.com

```shell
  $ sudo mkdir /opt/HES
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```
**[Note]** Internet connection required to download NuGet packages

## 2.3 Hideez Enterprise Server Configuration

Edit the file
`/opt/HES/<Name_Of_Domain>/appsettings.json`

The following is an example of how to open a configuration file for editing, for the case when the domain is hideez.example.com:

```shell
  $ sudo vi /opt/HES/hideez.example.com/appsettings.json
```

```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "server=<mysql_server>;port=<mysql_port>;database=<your_db>;uid=<your_user>;pwd=<your_secret>"
  },

  "EmailSender": {
    "Host": "<email_host>",
    "Port": "<email_port>",
    "EnableSSL": true,
    "UserName": "<your_email_name>",
    "Password": "<your_email_password>"
  },
  
  "DataProtection": {
    "Password": "<protection_password>"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information"
    }
  },

  "AllowedHosts": "*"
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

After saving the settings file, you can check that HES server is up and running:
```shell
  $ cd /opt/HES/<Name_Of_Domain>
  $ sudo ./HES.Web 
```
The output should look something like this:

```shell
Hosting environment: Production
Content root path: /opt/HES/hideez.example.com
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
Application started. Press Ctrl+C to shut down.
```
this means that HES server is successfully configured and started.

## 2.4 Daemonizing of Enterprise Server

Important note. By default, .net Core uses ports 5000 and 5001. Therefore, if only one domain 
is running on the server, port numbers can be skipped. But if it is supposed to run a few sites
on one computer, then it is necessary to specify different ports for each site.

Create the file `/lib/systemd/system/HES-<Name_Of_Domain>.service`


Below is an example for hideez.example.com, ports 5000 è 5001:

```shell
  $ sudo cat > /lib/systemd/system/HES-hideez.example.com.service << EOF
[Unit]
  Description=hideez.example.com Hideez Enterprise Service

[Service]

  User=root
  Group=root

  WorkingDirectory=/opt/HES/hideez.example.com
  ExecStart=/opt/HES/hideez.example.com/HES.Web --server.urls "http://localhost:5000;https://localhost:5001"
  Restart=on-failure
  ExecReload=/bin/kill -HUP $MAINPID
  KillMode=process
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

[Install]
  WantedBy=multi-user.target
EOF
```

if there will be only one HES service per server, you can omit parameter
--server.urls "http://localhost:5000;https://localhost:5001"

**enabling autostart (using hideez.example.com as an example)**

```shell
  $ sudo systemctl enable HES-hideez.example.com.service
  $ sudo systemctl restart HES-hideez.example.com.service
```

You can verify that HES server is running with the command

```shell
sudo systemctl status HES-hideez.example.com

```
(of course, instead of "HES-hideez.example.com", there should be a service name created earlier)

The output of the command should be something like this:

```shell
* HES-hideez.example.com.service - hideez.example.com Hideez Enterprise Service
   Loaded: loaded (/usr/lib/systemd/system/HES-hideez.example.com.service; enabled; vendor preset: disabled)
   Active: active (running) since Sat 2020-01-25 10:31:13 UTC; 54min ago
 Main PID: 12976 (HES.Web)
   CGroup: /system.slice/HES-hideez.example.com.service
            +-12976 /opt/HES/hideez.example.com/HES.Web

Jan 25 10:31:13 HESServerTest systemd[1]: Started hideez.example.com Hideez Enterprise Service.
Jan 25 10:31:22 HESServerTest HES.Web[12976]: Hosting environment: Production
Jan 25 10:31:22 HESServerTest HES.Web[12976]: Content root path: /opt/HES/hideez.example.com
Jan 25 10:31:22 HESServerTest HES.Web[12976]: Now listening on: http://localhost:5000
Jan 25 10:31:22 HESServerTest HES.Web[12976]: Now listening on: https://localhost:5001
Jan 25 10:31:22 HESServerTest HES.Web[12976]: Application started. Press Ctrl+C to shut down.
```

## 2.5 Reverse proxy configuration
To access your server from the local network as well as from the Internet, you have to configure a reverse proxy.

 Creating a Self-Signed SSL Certificate for Nginx
 Note For a "real" site, you should take care of acquiring a certificate from a certificate authority.
 For self-signed certificate, browser will alert you that site has security issues.
 
 Below is an example for hideez.example.com
 (when generating a certificate, answer a few simple questions)

```shell
 $ sudo mkdir /etc/nginx/certs
 $ sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/nginx/certs/hideez.example.com.key -out /etc/nginx/certs/hideez.example.com.crt
```

  Basic Configuration for the Nginx Reverse Proxy

```conf
    server {
        listen       80 default_server;
        listen       [::]:80 default_server;
        server_name  hideez.example.com;


            # Enable proxy websockets for the Hideez Client to work
            proxy_http_version 1.1;
            proxy_buffering off;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
            proxy_pass http://localhost:5000;
        }
  ...
```

```conf
    server {
        listen       443 ssl http2 default_server;
        listen       [::]:443 ssl http2 default_server;
        server_name  hideez.example.com;



    server {
          server_name hideez.example.com;
          listen [::]:443 ssl ;
          listen 443 ssl; 
          ssl_certificate "certs/hideez.example.com.crt";
          ssl_certificate_key "certs/hideez.example.com.key";

          location / {
                 proxy_pass https://localhost:5001;
                 proxy_http_version 1.1;
                 proxy_set_header Upgrade $http_upgrade;
                 proxy_set_header  Connection $connection_upgrade;
                 proxy_set_header Host $host;
                 proxy_cache_bypass $http_upgrade;
                 proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
                 proxy_set_header X-Forwarded-Proto $scheme;
          }

            # Enable proxy websockets for the hideez Client to work
            proxy_http_version 1.1;
            proxy_buffering off;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
            proxy_pass https://localhost:5001;
        }
  ...
```

After saving the file, it is recommended to check nginx settings:
```shell
  $ sudo nginx -t
```
The output should be something like this:
```shell
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```
Otherwise, you should carefully review the settings and correct the errors.

** Restarting Nginx and checking its status **

```shell
  $ sudo systemctl restart nginx
  $ sudo systemctl status nginx
  * nginx.service - The nginx HTTP and reverse proxy server
   Loaded: loaded (/usr/lib/systemd/system/nginx.service; enabled; vendor preset: disabled)
   Active: active (running) since Sat 2020-01-25 11:23:46 UTC; 8s ago
  Process: 13093 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
  Process: 13091 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
  Process: 13089 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
 Main PID: 13095 (nginx)
   CGroup: /system.slice/nginx.service
           +-13095 nginx: master process /usr/sbin/nginx
           +-13096 nginx: worker process
```

## 2.6 Firewall Configuration

To access the server from the network, ports 22, 80, and 443 should be opened:

*CentOS*
```shell
$ sudo firewall-cmd --zone=public --permanent --add-port=22/tcp
$ sudo firewall-cmd --zone=public --permanent --add-port=80/tcp
$ sudo firewall-cmd --zone=public --permanent --add-port=443/tcp
$ sudo firewall-cmd --reload
```
*Ubuntu*
```shell
$ sudo ufw allow 22
$ sudo ufw allow 80
$ sudo ufw allow 443
$ sudo ufw enable
```

Setup is complete. The server should be accessible in a browser at the address `https://<Name_Of_Domain>`

## Updating HES

### 1. Updating sources from GitHub repository

```shell
  $ cd /opt/src/HES
  $ sudo git pull
```

### 2. Back up Hideez Enterprise Server

```shell
  $ sudo systemctl stop hideez.service
  $ sudo mv /opt/HES /opt/HES.old
```

### 3. Build a new version of Hideez Enterprise Server from sources

```shell
  $ sudo mkdir /opt/HES
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```

### 4. Back up MySQL Database (optional)

```shell
  $ sudo mkdir /opt/backups && cd /opt/backups
  $ sudo mysqldump -u <your_user> -p <your_secret> <your_db> | gzip -c > <your_db>.sql.gz
```

### 5. Copy your configuration file

```shell
  $ sudo cp /opt/HES.old/appsettings.json /opt/HES/appsettings.json
```

### 6. Restart Hideez Enterprise Server and check its status

```shell
  $ sudo systemctl restart hideez.service
  $ sudo systemctl status hideez.service
  * hideez.service - Hideez Web service
     Loaded: loaded (/usr/lib/systemd/system/hideez.service; enabled; vendor preset: disabled)
     Active: active (running) since Tue 2019-11-05 15:34:39 EET; 2 weeks 2 days ago
   Main PID: 10816 (HES.Web)
     CGroup: /system.slice/hideez.service
             +-10816 /opt/HES/HES.Web
```
