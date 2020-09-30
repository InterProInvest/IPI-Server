Hideez Enterprise Server
========================

The IPI Server is a web application intended to manage the IPI Keys. IPI Key is a hardware security token intended to store users' credentials. It has Bluetooth Low Energy (BLE) interface to connect to host computers. It can be used to unlock and lock Windows PC by BLE proximity (distance between the user and his PC) and as a hardware password manager and One Time Password (OTP) generator. IPI Client application is used to connect and manage IPI Keys on a local PC. IPI Client can lock/unlock your PC and enter your logins, passwords, and OTP into web sites and desktop applications, getting them from the IPI Key. With the IPI Server, you can securely transfer credentials from the web directly to a IPI Key; manage employee list; assign IPI Keys to an employee; securely transfer credentials to the IPI Key; view statistics and audit reports; manage shared accounts and more. To start working with IPI Server, you need:
- At least one IPI Key and one IPI Dongle (USB radio module to connect the Key). You can order it from the [IPI web site] (https://interproinvest.com/).
- Linux or Windows machine to install the IPI Server.
- Windows 7/10 PC to install IPI Client application – you can build it from sources or request from the IPI.
- Windows Active Directory domain (Optionally).

[Learn about Hideez Enterprise Solution](https://hideez.com/pages/hideez-enterprise).

User documention about [Hideez Enterprise Solution](https://support.hideez.com/hideez-enterprise-server). 

## Supported platforms

At this time, IPI Server can be installed on Linux or Windows computer. IPI Client can be installed on Windows 10 PC. Currently, we don't support mobile or Mac platforms but stay tuned for the updates.

## System requirements

IPI Server: Linux or Windows with .NET Core (.NET Core SDK version 3.1) installed, MySQL database server. More requirements see on the corresponding platform setup instructions. IPI Client: Windows 10, one free USB port for a Hideez Dongle.

## Get Started

To deploy the server on Linux, follow instructions in the [Linux deployment](LINUX.md).

To deploy the server on Linux from a Docker containar, follow instructions in the [Linux Docker deployment](HES.Docker/README.md).

To deploy the server on Windows, follow instructions in the [Windows deployment](WINDOWS.md).

After you have deployed the server open the IPI Server web interface and log in using the default login 'admin@hideez.com' and default password 'admin'.

## API documentation

Almost everything you can do with the IPI Server web interface you can do via the IPI Server REST API calls. Follow the `Quick Start` instructions in the [documentation](API.md).

## Next steps

- invite more admins and delete default login 'admin@hideez.com'
- setup the IPI Server following the [user documentation](https://support.hideez.com/hideez-enterprise-server)
- if you use Linux and need the AD integration, [join your Linux server to the AD](LINUX_AD.md) 

## Related projects

These are repos for related projects:
* [Hideez Client](https://github.com/HideezGroup/win.HideezSafe) - Connection with the device, entering credentials and OTP.
