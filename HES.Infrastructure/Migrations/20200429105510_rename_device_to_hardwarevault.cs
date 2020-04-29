using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_device_to_hardwarevault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // WorkstationEvents
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Accounts_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Devices_DeviceId",
                table: "WorkstationEvents");
            // WorkstationSessions
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Accounts_DeviceAccountId",
                table: "WorkstationSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationSessions_Devices_DeviceId",
                table: "WorkstationSessions");
            // Devices
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
               name: "FK_Devices_Employees_EmployeeId",
               table: "Devices");
            // DeviceTasks
            migrationBuilder.DropForeignKey(
               name: "FK_DeviceTasks_Accounts_AccountId",
               table: "DeviceTasks");
            // ProximityDevices
            migrationBuilder.DropForeignKey(
              name: "FK_ProximityDevices_Devices_DeviceId",
              table: "ProximityDevices");

            migrationBuilder.DropForeignKey(
             name: "FK_ProximityDevices_Workstations_WorkstationId",
             table: "ProximityDevices");
            // WorkstationSessions
            migrationBuilder.DropIndex(
                name: "IX_WorkstationSessions_DeviceAccountId",
                table: "WorkstationSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationSessions_DeviceId",
                table: "WorkstationSessions");
            // WorkstationEvents
            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_DeviceId",
                table: "WorkstationEvents");
            // Devices
            migrationBuilder.DropIndex(
                name: "IX_Devices_EmployeeId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_AcceessProfileId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_MAC",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_RFID",
                table: "Devices");
            // DeviceTasks
            migrationBuilder.DropIndex(
               name: "IX_DeviceTasks_AccountId",
               table: "DeviceTasks");
            // ProximityDevices
            migrationBuilder.DropIndex(
               name: "IX_ProximityDevices_DeviceId",
               table: "ProximityDevices");

            migrationBuilder.DropIndex(
                name: "IX_ProximityDevices_WorkstationId",
                table: "ProximityDevices"); 
            //
            migrationBuilder.RenameTable(
               name: "DeviceLicenses",
               newName: "HardwareVaultLicenses");

            migrationBuilder.RenameTable(
                name: "DeviceTasks",
                newName: "HardwareVaultTasks");

            migrationBuilder.RenameTable(
                name: "ProximityDevices",
                newName: "WorkstationProximityVaults");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "HardwareVaults");

            migrationBuilder.RenameTable(
                name: "DeviceAccessProfiles",
                newName: "HardwareVaultProfiles");

            migrationBuilder.RenameColumn(
                name: "DeviceAccountId",
                table: "WorkstationSessions",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "WorkstationSessions",
                newName: "HardwareVaultId");

            migrationBuilder.RenameColumn(
                name: "DeviceAccountId",
                table: "WorkstationEvents",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "WorkstationEvents",
                newName: "HardwareVaultId");

            migrationBuilder.RenameColumn(
               name: "AcceessProfileId",
               table: "HardwareVaults",
               newName: "HardwareVaultProfileId");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Templates");

            migrationBuilder.RenameColumn(
                 name: "DeviceId",
                 table: "HardwareVaultLicenses",
                 newName: "HardwareVaultId");

            migrationBuilder.RenameColumn(
                 name: "DeviceId",
                 table: "WorkstationProximityVaults",
                 newName: "HardwareVaultId");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                table: "HardwareVaultTasks",
                newName: "HardwareVaultId");

            //migrationBuilder.AddColumn<string>(
            //    name: "AccountId",
            //    table: "WorkstationSessions",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "HardwareVaultId",
            //    table: "WorkstationSessions",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "AccountId",
            //    table: "WorkstationEvents",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "HardwareVaultId",
            //    table: "WorkstationEvents",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "HardwareVaultLicenses",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        HardwareVaultId = table.Column<string>(nullable: false),
            //        LicenseOrderId = table.Column<string>(nullable: false),
            //        ImportedAt = table.Column<DateTime>(nullable: true),
            //        AppliedAt = table.Column<DateTime>(nullable: true),
            //        EndDate = table.Column<DateTime>(nullable: true),
            //        Data = table.Column<byte[]>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HardwareVaultLicenses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "HardwareVaultProfiles",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        Name = table.Column<string>(nullable: false),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        UpdatedAt = table.Column<DateTime>(nullable: true),
            //        ButtonBonding = table.Column<bool>(nullable: false),
            //        ButtonConnection = table.Column<bool>(nullable: false),
            //        ButtonNewChannel = table.Column<bool>(nullable: false),
            //        PinBonding = table.Column<bool>(nullable: false),
            //        PinConnection = table.Column<bool>(nullable: false),
            //        PinNewChannel = table.Column<bool>(nullable: false),
            //        MasterKeyBonding = table.Column<bool>(nullable: false),
            //        MasterKeyConnection = table.Column<bool>(nullable: false),
            //        MasterKeyNewChannel = table.Column<bool>(nullable: false),
            //        PinExpiration = table.Column<int>(nullable: false),
            //        PinLength = table.Column<int>(nullable: false),
            //        PinTryCount = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HardwareVaultProfiles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "HardwareVaultTasks",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        Password = table.Column<string>(nullable: true),
            //        OtpSecret = table.Column<string>(nullable: true),
            //        Operation = table.Column<int>(nullable: false),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        VaultId = table.Column<string>(nullable: true),
            //        AccountId = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HardwareVaultTasks", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_HardwareVaultTasks_Accounts_AccountId",
            //            column: x => x.AccountId,
            //            principalTable: "Accounts",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "HardwareVaults",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        MAC = table.Column<string>(nullable: false),
            //        Model = table.Column<string>(nullable: false),
            //        RFID = table.Column<string>(nullable: false),
            //        Battery = table.Column<int>(nullable: false),
            //        Firmware = table.Column<string>(nullable: false),
            //        Status = table.Column<int>(nullable: false),
            //        StatusReason = table.Column<int>(nullable: false),
            //        StatusDescription = table.Column<string>(nullable: true),
            //        LastSynced = table.Column<DateTime>(nullable: true),
            //        NeedSync = table.Column<bool>(nullable: false),
            //        EmployeeId = table.Column<string>(nullable: true),
            //        MasterPassword = table.Column<string>(nullable: true),
            //        HardwareVaultProfileId = table.Column<string>(nullable: false),
            //        ImportedAt = table.Column<DateTime>(nullable: false),
            //        HasNewLicense = table.Column<bool>(nullable: false),
            //        LicenseStatus = table.Column<int>(nullable: false),
            //        LicenseEndDate = table.Column<DateTime>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HardwareVaults", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_HardwareVaults_Employees_EmployeeId",
            //            column: x => x.EmployeeId,
            //            principalTable: "Employees",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_HardwareVaults_HardwareVaultProfiles_HardwareVaultProfileId",
            //            column: x => x.HardwareVaultProfileId,
            //            principalTable: "HardwareVaultProfiles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "WorkstationProximityVaults",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(nullable: false),
            //        HardwareVaultId = table.Column<string>(nullable: true),
            //        WorkstationId = table.Column<string>(nullable: true),
            //        LockProximity = table.Column<int>(nullable: false),
            //        UnlockProximity = table.Column<int>(nullable: false),
            //        LockTimeout = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_WorkstationProximityVaults", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_WorkstationProximityVaults_HardwareVaults_HardwareVaultId",
            //            column: x => x.HardwareVaultId,
            //            principalTable: "HardwareVaults",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //        table.ForeignKey(
            //            name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
            //            column: x => x.WorkstationId,
            //            principalTable: "Workstations",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_AccountId",
                table: "WorkstationSessions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_HardwareVaultId",
                table: "WorkstationSessions",
                column: "HardwareVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_AccountId",
                table: "WorkstationEvents",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_HardwareVaultId",
                table: "WorkstationEvents",
                column: "HardwareVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaults_EmployeeId",
                table: "HardwareVaults",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaults_HardwareVaultProfileId",
                table: "HardwareVaults",
                column: "HardwareVaultProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaults_MAC",
                table: "HardwareVaults",
                column: "MAC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaults_RFID",
                table: "HardwareVaults",
                column: "RFID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HardwareVaultTasks_AccountId",
                table: "HardwareVaultTasks",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityVaults_HardwareVaultId",
                table: "WorkstationProximityVaults",
                column: "HardwareVaultId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityVaults_WorkstationId",
                table: "WorkstationProximityVaults",
                column: "WorkstationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Accounts_AccountId",
                table: "WorkstationEvents",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_HardwareVaults_HardwareVaultId",
                table: "WorkstationEvents",
                column: "HardwareVaultId",
                principalTable: "HardwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_Accounts_AccountId",
                table: "WorkstationSessions",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationSessions_HardwareVaults_HardwareVaultId",
                table: "WorkstationSessions",
                column: "HardwareVaultId",
                principalTable: "HardwareVaults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
              name: "FK_HardwareVaults_Employees_EmployeeId",
              table: "HardwareVaults",
              column: "EmployeeId",
              principalTable: "Employees",
              principalColumn: "Id",
              onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
              name: "FK_HardwareVaults_HardwareVaultProfiles_AcceessProfileId",
              table: "HardwareVaults",
              column: "HardwareVaultProfileId",
              principalTable: "HardwareVaultProfiles",
              principalColumn: "Id",
              onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
             name: "FK_HardwareVaultTasks_Accounts_AccountId",
             table: "HardwareVaultTasks",
             column: "AccountId",
             principalTable: "Accounts",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.AddForeignKey(
             name: "FK_WorkstationProximityVaults_HardwareVaults_HardwareVaultId",
             table: "WorkstationProximityVaults",
             column: "HardwareVaultId",
             principalTable: "HardwareVaults",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
             name: "FK_WorkstationProximityVaults_Workstations_WorkstationId",
             table: "WorkstationProximityVaults",
             column: "WorkstationId",
             principalTable: "Workstations",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        //    migrationBuilder.DropForeignKey(
        //        name: "FK_WorkstationEvents_Accounts_AccountId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.DropForeignKey(
        //        name: "FK_WorkstationEvents_HardwareVaults_HardwareVaultId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.DropForeignKey(
        //        name: "FK_WorkstationSessions_Accounts_AccountId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropForeignKey(
        //        name: "FK_WorkstationSessions_HardwareVaults_HardwareVaultId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropTable(
        //        name: "HardwareVaultLicenses");

        //    migrationBuilder.DropTable(
        //        name: "HardwareVaultTasks");

        //    migrationBuilder.DropTable(
        //        name: "WorkstationProximityVaults");

        //    migrationBuilder.DropTable(
        //        name: "HardwareVaults");

        //    migrationBuilder.DropTable(
        //        name: "HardwareVaultProfiles");

        //    migrationBuilder.DropIndex(
        //        name: "IX_WorkstationSessions_AccountId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropIndex(
        //        name: "IX_WorkstationSessions_HardwareVaultId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropIndex(
        //        name: "IX_WorkstationEvents_AccountId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.DropIndex(
        //        name: "IX_WorkstationEvents_HardwareVaultId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.DropColumn(
        //        name: "AccountId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropColumn(
        //        name: "HardwareVaultId",
        //        table: "WorkstationSessions");

        //    migrationBuilder.DropColumn(
        //        name: "AccountId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.DropColumn(
        //        name: "HardwareVaultId",
        //        table: "WorkstationEvents");

        //    migrationBuilder.AddColumn<string>(
        //        name: "DeviceAccountId",
        //        table: "WorkstationSessions",
        //        type: "varchar(255) CHARACTER SET utf8mb4",
        //        nullable: true);

        //    migrationBuilder.AddColumn<string>(
        //        name: "DeviceId",
        //        table: "WorkstationSessions",
        //        type: "varchar(255) CHARACTER SET utf8mb4",
        //        nullable: true);

        //    migrationBuilder.AddColumn<string>(
        //        name: "DeviceAccountId",
        //        table: "WorkstationEvents",
        //        type: "varchar(255) CHARACTER SET utf8mb4",
        //        nullable: true);

        //    migrationBuilder.AddColumn<string>(
        //        name: "DeviceId",
        //        table: "WorkstationEvents",
        //        type: "varchar(255) CHARACTER SET utf8mb4",
        //        nullable: true);

        //    migrationBuilder.AddColumn<bool>(
        //        name: "Deleted",
        //        table: "Templates",
        //        type: "tinyint(1)",
        //        nullable: false,
        //        defaultValue: false);

        //    migrationBuilder.CreateTable(
        //        name: "DeviceAccessProfiles",
        //        columns: table => new
        //        {
        //            Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            ButtonBonding = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            ButtonConnection = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            ButtonNewChannel = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
        //            MasterKeyBonding = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            MasterKeyConnection = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            MasterKeyNewChannel = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            Name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
        //            PinBonding = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            PinConnection = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            PinExpiration = table.Column<int>(type: "int", nullable: false),
        //            PinLength = table.Column<int>(type: "int", nullable: false),
        //            PinNewChannel = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            PinTryCount = table.Column<int>(type: "int", nullable: false),
        //            UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_DeviceAccessProfiles", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "DeviceLicenses",
        //        columns: table => new
        //        {
        //            Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            AppliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
        //            Data = table.Column<byte[]>(type: "longblob", nullable: true),
        //            DeviceId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
        //            EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
        //            ImportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
        //            LicenseOrderId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_DeviceLicenses", x => x.Id);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "DeviceTasks",
        //        columns: table => new
        //        {
        //            Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            AccountId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
        //            CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
        //            DeviceId = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
        //            Operation = table.Column<int>(type: "int", nullable: false),
        //            OtpSecret = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
        //            Password = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_DeviceTasks", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_DeviceTasks_Accounts_AccountId",
        //                column: x => x.AccountId,
        //                principalTable: "Accounts",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "Devices",
        //        columns: table => new
        //        {
        //            Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            AcceessProfileId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            Battery = table.Column<int>(type: "int", nullable: false),
        //            EmployeeId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
        //            Firmware = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
        //            HasNewLicense = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            ImportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
        //            LastSynced = table.Column<DateTime>(type: "datetime(6)", nullable: true),
        //            LicenseEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
        //            LicenseStatus = table.Column<int>(type: "int", nullable: false),
        //            MAC = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            MasterPassword = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
        //            Model = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
        //            NeedSync = table.Column<bool>(type: "tinyint(1)", nullable: false),
        //            RFID = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            Status = table.Column<int>(type: "int", nullable: false),
        //            StatusDescription = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
        //            StatusReason = table.Column<int>(type: "int", nullable: false)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_Devices", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
        //                column: x => x.AcceessProfileId,
        //                principalTable: "DeviceAccessProfiles",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Cascade);
        //            table.ForeignKey(
        //                name: "FK_Devices_Employees_EmployeeId",
        //                column: x => x.EmployeeId,
        //                principalTable: "Employees",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateTable(
        //        name: "ProximityDevices",
        //        columns: table => new
        //        {
        //            Id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
        //            DeviceId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true),
        //            LockProximity = table.Column<int>(type: "int", nullable: false),
        //            LockTimeout = table.Column<int>(type: "int", nullable: false),
        //            UnlockProximity = table.Column<int>(type: "int", nullable: false),
        //            WorkstationId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: true)
        //        },
        //        constraints: table =>
        //        {
        //            table.PrimaryKey("PK_ProximityDevices", x => x.Id);
        //            table.ForeignKey(
        //                name: "FK_ProximityDevices_Devices_DeviceId",
        //                column: x => x.DeviceId,
        //                principalTable: "Devices",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //            table.ForeignKey(
        //                name: "FK_ProximityDevices_Workstations_WorkstationId",
        //                column: x => x.WorkstationId,
        //                principalTable: "Workstations",
        //                principalColumn: "Id",
        //                onDelete: ReferentialAction.Restrict);
        //        });

        //    migrationBuilder.CreateIndex(
        //        name: "IX_WorkstationSessions_DeviceAccountId",
        //        table: "WorkstationSessions",
        //        column: "DeviceAccountId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_WorkstationSessions_DeviceId",
        //        table: "WorkstationSessions",
        //        column: "DeviceId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_WorkstationEvents_DeviceAccountId",
        //        table: "WorkstationEvents",
        //        column: "DeviceAccountId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_WorkstationEvents_DeviceId",
        //        table: "WorkstationEvents",
        //        column: "DeviceId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Devices_AcceessProfileId",
        //        table: "Devices",
        //        column: "AcceessProfileId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Devices_EmployeeId",
        //        table: "Devices",
        //        column: "EmployeeId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Devices_MAC",
        //        table: "Devices",
        //        column: "MAC",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_Devices_RFID",
        //        table: "Devices",
        //        column: "RFID",
        //        unique: true);

        //    migrationBuilder.CreateIndex(
        //        name: "IX_DeviceTasks_AccountId",
        //        table: "DeviceTasks",
        //        column: "AccountId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProximityDevices_DeviceId",
        //        table: "ProximityDevices",
        //        column: "DeviceId");

        //    migrationBuilder.CreateIndex(
        //        name: "IX_ProximityDevices_WorkstationId",
        //        table: "ProximityDevices",
        //        column: "WorkstationId");

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_WorkstationEvents_Accounts_DeviceAccountId",
        //        table: "WorkstationEvents",
        //        column: "DeviceAccountId",
        //        principalTable: "Accounts",
        //        principalColumn: "Id",
        //        onDelete: ReferentialAction.Restrict);

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_WorkstationEvents_Devices_DeviceId",
        //        table: "WorkstationEvents",
        //        column: "DeviceId",
        //        principalTable: "Devices",
        //        principalColumn: "Id",
        //        onDelete: ReferentialAction.Restrict);

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_WorkstationSessions_Accounts_DeviceAccountId",
        //        table: "WorkstationSessions",
        //        column: "DeviceAccountId",
        //        principalTable: "Accounts",
        //        principalColumn: "Id",
        //        onDelete: ReferentialAction.Restrict);

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_WorkstationSessions_Devices_DeviceId",
        //        table: "WorkstationSessions",
        //        column: "DeviceId",
        //        principalTable: "Devices",
        //        principalColumn: "Id",
        //        onDelete: ReferentialAction.Restrict);
        }
    }
}