﻿using System.Reflection;

namespace HES.Core.Constants
{
    public class ServerConstants
    {
        // Server Version
        public static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        // Hardware Vault default profile id
        public const string DefaulHardwareVaultProfileId = "default";

        // App Settings keys
        public const string Licensing = "licensing";
        public const string Alarm = "alarm";
        public const string Domain = "domain";
        public const string Email = "email";
        public const string Server = "server";
    }
}