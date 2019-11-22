# Windows deployment

## Requirements

  * Git
  * .NET Core (.NET Core SDK version 2.2).
  * MySQL Server (version 8.0+).

## Preparation System

Download and install Git: https://git-scm.com/download/win

Download and install .NET Core SDK 2.2: https://dotnet.microsoft.com/download/dotnet-core/2.2

Download and install MySQL: https://dev.mysql.com/downloads/installer/   
- [MySQL Installer Initial Setup](https://dev.mysql.com/doc/refman/8.0/en/mysql-installer.html)

## Getting Started.

### 1. Creating MySQL User and Database for Hideez Enterprise Server

Configuring MySQL Server
  
```shell
  mysql -h localhost -u root -p
```

```sql
  ### CREATE DATABASE
  mysql> CREATE DATABASE <your_db>

  ### CREATE USER ACCOUNT
  mysql> CREATE USER '<your_user>'@'127.0.0.1' IDENTIFIED BY '<your_secret>'

  ### GRANT PERMISSIONS ON DATABASE
  mysql> GRANT ALL ON <your_db>.* TO '<your_user>'@'127.0.0.1'

  ###  RELOAD PRIVILEGES
  mysql> FLUSH PRIVILEGES
```

### 2. Cloning a GitHub Repository

```shell
  > md Hideez
  > cd Hideez
  > git clone https://github.com/HideezGroup/HES src
  > cd src\HES.Web
```

### 3. Compiling Hideez Enterprise Server

```shell
  > md ..\..\HES
  > dotnet publish -c release -v d -o "..\..\HES" --framework netcoreapp2.2 --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Require internet connectivity

### 4. Configuring Hideez Enterprise Server

```shell
  > cd..\..\HES
  > appsettings.json
```

```json
  {
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=<your_db>;uid=<your_user>;pwd=<your_secret>"
  },

  "EmailSender": {
    "Host": "smtp.example.com",
    "Port": 123,
    "EnableSSL": true,
    "UserName": "user@example.com",
    "Password": "password"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information"
    }
  },

  "AllowedHosts": "*"
```

### 5. Configuring IIS

If the web server is not enabled then use the [official guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2#iis-configuration)
 
Create a Self-Signed Certificate for IIS

- Start **IIS Manager**. For information about starting IIS Manager, see [Open IIS Manager](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN).
- Click on the name of the server in the Connections column on the left. Double-click on **Server Certificates**.
- In the Actions column on the right, click on **Create Self-Signed Certificate...**
- Enter any *friendly* name and then click **OK**.
- You will now have an IIS Self Signed Certificate valid for 1 year listed under Server Certificates. The certificate common name (Issued To) is the server name.

Add a Web Site

- Start **IIS Manager**. For information about starting IIS Manager, see [Open IIS Manager](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN).
- In the **Connections** pane, right-click the **Sites** node in the tree view, and then click **Add Web Site**.
- In the **Add Web Site** dialog box, type a *friendly* name for your Web site in the **Web site name** box.
- If you want to select a different application pool than the one listed in the Application Pool box. In the **Select Application Pool** dialog box, select an application pool from the **Application Pool** list, and then click **OK**.
- In the **Physical path** box, type the *physical path* of the Web site's folder, or click the browse button **(...)** to browse the file system to find the folder.
- Select the protocol for the Web site from the **Type** list.
- The default value in the **IP address** box is **All Unassigned**. If you must specify a static IP address for the Web site, type the IP *address* in the **IP address** box.
- Type a port number in the **Port** text box.
- Optionally, type a host header name for the Web site in the **Host Header** box.
- If you do not have to make any changes to the site, and you want the Web site to be immediately available, select the **Start Web site immediately** check box.
- Click **OK**.
- Under the server's node, select **Application Pools**.
- Right-click the site's app pool and select **Basic Settings** from the contextual menu.
- In the **Edit Application Pool** window, set the **.NET CLR version** to **No Managed Code**.

## Updating

### 1. Updating Your Repo by Setting Up a Remote

```shell
  > cd Hideez\src
  > git pull
```

### 2. Backuping Hideez Enterprise Server

```shell
  > cd %windir%\system32\inetsrv
  > appcmd stop site /site.name:<site_name>
  > cd Hideez 
  > rename HES HES.old
```

### 3. Compiling Hideez Enterprise Server

```shell
  > cd src\HES.Web
  > md ..\..\HES
  > dotnet publish -c release -v d -o "..\..\HES" --framework netcoreapp2.2 --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Require internet connectivity

### 4. Restoring configure file Hideez Enterprise Server

```shell
  > cd ..\..\
  > copy HES.old\appsettings.json HES\appsettings.json
  > Overwrite HES\appsettings.json? (Yes/No/All): y 
  > rmdir /s HES.old
  > HES.old, Are you sure (Y/N)? y
```

### 5. Starting Hideez Enterprise Server

```shell
  > appcmd start site /site.name:<site_name>  
```
