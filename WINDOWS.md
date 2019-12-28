# Windows deployment

## Requirements

  * Git.
  * .NET Core (.NET Core SDK version 2.2).
  * MySQL Server (version 8.0+).

## System Preparation

#### If the web server is not enabled then use the [official guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2#iis-configuration) to enable IIS.

Download and install [Git](https://git-scm.com/download/win)

Download and install [.NET Core SDK 2.2 and Hosting Bundle](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- [.NET Core SDK 2.2](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-2.2.207-windows-x64-installer)
- [Windows Hosting Bundle](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-aspnetcore-2.2.8-windows-hosting-bundle-installer)

Download and install [MySQL](https://dev.mysql.com/downloads/installer/)
- [Docs MySQL initial setup](https://dev.mysql.com/doc/refman/8.0/en/mysql-installer.html)

## Getting Started (fresh install)

### 1. Cloning the HES GitHub repository

```shell
  > cd C:\
  > md Hideez
  > cd Hideez
  > git clone https://github.com/HideezGroup/HES src
  > cd src\HES.Web
```

### 2. Building the HES from the sources

```shell
  > md ..\..\HES
  > dotnet publish -c release -v d -o "..\..\HES" --framework netcoreapp2.2 --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Requires internet connectivity to download NuGet packages

### 3. Configuring HES

```shell
  > cd..\..\HES
  > notepad appsettings.json
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

### 4. Configuring IIS
 
Create a Self-Signed Certificate for IIS

- Start **IIS Manager**. For information about starting IIS Manager, see [Open IIS Manager](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-server-2008-R2-and-2008/cc770472(v=ws.10)?redirectedfrom=MSDN).
- Click on the name of the server in the Connections column on the left. Double-click on **Server Certificates**.
- In the Actions column on the right, click on **Create Self-Signed Certificate...**
- Enter any *friendly* name and then click **OK**.
- You will now have an IIS Self Signed Certificate valid for 1 year listed under Server Certificates. The certificate common name (Issued To) is the server name.

Add the Web Site

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

### 1. Updating the sources from the GitHub repository

```shell
  > cd C:\Hideez\src
  > git pull
```

### 2. Backing up the HES binaries

```shell
  > cd %windir%\system32\inetsrv
  > appcmd stop site /site.name:<site_name>
  > cd C:\Hideez 
  > rename HES HES.old
```

### 3. Building the HES from the sources

```shell
  > cd src\HES.Web
  > md ..\..\HES
  > dotnet publish -c release -v d -o "..\..\HES" --framework netcoreapp2.2 --runtime win-x64 HES.Web.csproj
```
  * **[Note]** Requires internet connectivity to download NuGet packages

### 4. Restoring the configuration file

```shell
  > cd ..\..\
  > copy HES.old\appsettings.json HES\appsettings.json
  > Overwrite HES\appsettings.json? (Yes/No/All): y 
  > rmdir /s HES.old
  > HES.old, Are you sure (Y/N)? y
```

### 5. Backuping MySQL Database (optional)

```shell
  > md bkp
  > cd C:\Program Files\MySQL\MySQL Server 8.0\bin
  > mysqldump -u <your_user> -p <your_db> > C:\Hideez\<your_db_bkp>.sql
  Enter password: ********
```

### 6. Starting the HES

```shell
  > cd %windir%\system32\inetsrv
  > appcmd start site /site.name:<site_name>  
```
