# Linux deployment

## Requirements

  * Git
  * .NET Core (.NET Core SDK version 2.2).
  * MySQL Server (version 8.0+).

## System Preparation (Example for [CentOS 7](https://www.centos.org/about/))

  Disabling SELinux:

```shell
  $ sudo sed -i 's/SELINUX=enforcing/SELINUX=disabled/' /etc/sysconfig/selinux
  $ sudo setenforce 0
```
  Installing EPEL Repository and Nginx

```shell
  $ sudo yum install epel-release
  $ sudo yum install nginx
  $ sudo systemctl enable nginx
```

  Adding Microsoft Package Repository and Installing .NET Core:

```shell
  $ sudo rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
  $ sudo yum install dotnet-sdk-2.2
```

  Adding MySQL Package Repository and Installing MySQL Server:

```shell
  $ sudo rpm -Uvh https://dev.mysql.com/get/mysql80-community-release-el7-3.noarch.rpm
  $ sudo yum install mysql-server
```

  ## Getting Started (fresh install)

### 1. Postinstalling and Securing MySQL Server

```shell
  $ sudo mysql_secure_installation
```

  It is recommended to say yes to the following questions:

```shell
  Enter password for user root:

  The existing password for the user account root has expired. Please set a new password.

  New password:
  Re-enter new password:

  Remove anonymous users? (Press y|Y for Yes, any other key for No) : y

  Disallow root login remotely? (Press y|Y for Yes, any other key for No) : y

  Remove test database and access to it? (Press y|Y for Yes, any other key for No) : y

  Reload privilege tables now? (Press y|Y for Yes, any other key for No) : y
```
  * **[Note]** Find default root password using `sudo grep "A temporary password" /var/log/mysqld.log`

  Before starting mysql server for the first time, you must set lower_case_table_names = 1 in the /etc/my.cnf file:
  
```shell
  $ sudo echo "lower_case_table_names=1" >> /etc/my.cnf  
```

  Enabling and running MySQL Service

```shell
  $ sudo systemctl restart mysqld.service
  $ sudo systemctl enable mysqld.service
```

### 2. Creating MySQL User and Database for the Hideez Enterprise Server

  Configuring MySQL Server

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

### 3. Installing and cloning the HES GitHub repository

```shell
  $ sudo yum install git && cd /opt
  $ sudo git clone https://github.com/HideezGroup/HES src && cd src/HES.Web/
```

### 4. Building the Hideez Enterprise Server from the sources

```shell
  $ sudo mkdir /opt/HES
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```
  * **[Note]** Requires internet connectivity to download NuGet packages


### 5. Configuring the Hideez Enterprise Server (MySQL credentials)

```shell
  $ sudo vi /opt/HES/appsettings.json
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

* **<mysql_server>** - MySQL server ip address (example `127.0.0.1`)
* **<mysql_port>** - MySQL server port (example `3306`)
* **<your_db>** - The name of your database on the MySQL server (example `hes`)
* **<your_user>** - MySQL database username (example `admin`)
* **<your_secret>** - Password from database user on MySQL server (example `password`)
* **<email_host>** - Host your email server (example `smtp.example.com`)
* **<email_port>** - Port your email server (example `123`)
* **<your_email_name>** - Your email name (example `user@example.com`)
* **<your_email_password>** - Your email name (example `password`)
* **<protection_password>** - Your password for database encryption (example `password`)

### 6. Daemonizing Hideez Enterprise Server

```shell
  $ sudo cat <<EOF > /lib/systemd/system/hideez.service
  [Unit]
  Description=Hideez Enterprise Service

  [Service]

  User=root
  Group=root

  WorkingDirectory=/opt/HES
  ExecStart=/opt/HES/HES.Web
  Restart=on-failure
  ExecReload=/bin/kill -HUP $MAINPID
  KillMode=process
  # SyslogIdentifier=dotnet-sample-service
  # PrivateTmp=true

  [Install]
  WantedBy=multi-user.target
  EOF

  $ sudo systemctl enable hideez.service
  $ sudo systemctl restart hideez.service
```

### 7. Configuring the Nginx Reverse Proxy

  Creating a Self-Signed SSL Certificate for the Nginx

```shell
 $ sudo mkdir /etc/nginx/certs
 $ sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout /etc/nginx/certs/hideez.key -out /etc/nginx/certs/hideez.crt
```

  Basic Configuration for the Nginx Reverse Proxy

```conf
    server {
        listen       80 default_server;
        listen       [::]:80 default_server;
        server_name  hideez.example.com;

        location / {
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

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

        ssl_certificate "certs/hideez.crt";
        ssl_certificate_key "certs/hideez.key";

        location / {
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

            # Enable proxy websockets for the hideez Client to work
            proxy_http_version 1.1;
            proxy_buffering off;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection $http_connection;
            proxy_pass https://localhost:5001;
        }
  ...
```

  Restarting the Nginx Reverse Proxy and check its status

```shell
  $ sudo systemctl restart nginx
  $ sudo systemctl status nginx
  ● nginx.service - The nginx HTTP and reverse proxy server
     Loaded: loaded (/usr/lib/systemd/system/nginx.service; disabled; vendor preset: disabled)
     Active: active (running) since Fri 2019-11-08 20:46:28 EET; 6min ago
    Process: 14756 ExecStart=/usr/sbin/nginx (code=exited, status=0/SUCCESS)
    Process: 14754 ExecStartPre=/usr/sbin/nginx -t (code=exited, status=0/SUCCESS)
    Process: 14752 ExecStartPre=/usr/bin/rm -f /run/nginx.pid (code=exited, status=0/SUCCESS)
   Main PID: 14758 (nginx)
     CGroup: /system.slice/nginx.service
             ├─14758 nginx: master process /usr/sbin/nginx
             └─14760 nginx: worker process
```
## Updating the HES

### 1. Updating the sources from the GitHub repository

```shell
  $ cd /opt/src
  $ sudo git pull
```

### 2. Backing up the Hideez Enterprise Server

```shell
  $ sudo systemctl stop hideez.service
  $ sudo mv /opt/HES /opt/HES.old
```

### 3. Building the Hideez Enterprise Server from the sources

```shell
  $ sudo mkdir /opt/HES
  $ sudo dotnet publish -c release -v d -o "/opt/HES" --framework netcoreapp2.2 --runtime linux-x64 HES.Web.csproj
  $ sudo cp /opt/src/HES.Web/Crypto_linux.dll /opt/HES/Crypto.dll
```
  * **[Note]** Requires internet connectivity to download NuGet packages

### 4. Backuping MySQL Database (optional)

```shell
  $ sudo mkdir /opt/backups && cd /opt/backups
  $ sudo mysqldump -u <your_user> -p <your_secret> <your_db> | gzip -c > <your_db> .sql.gz
```

### 5.  Restoring the configuration file

```shell
  $ sudo cp /opt/HES.old/appsettings.json /opt/HES/appsettings.json
  $ sudo rm -rf /opt/HES.old
```

### 6. Restarting the Hideez Enterprise Server and check its status

```shell
  $ sudo systemctl restart hideez.service
  $ sudo systemctl status hideez.service
  ● hideez.service - Hideez Web service
     Loaded: loaded (/usr/lib/systemd/system/hideez.service; enabled; vendor preset: disabled)
     Active: active (running) since Tue 2019-11-05 15:34:39 EET; 2 weeks 2 days ago
   Main PID: 10816 (HES.Web)
     CGroup: /system.slice/hideez.service
             └─10816 /opt/HES/HES.Web
```
