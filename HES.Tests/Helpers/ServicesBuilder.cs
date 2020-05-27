using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HES.Tests.Helpers
{
    public class ServicesBuilder
    {
        private IAsyncRepository<Employee> employeeRepository;
        private IAsyncRepository<HardwareVault> hardwareVaultRepository;
        private IAsyncRepository<HardwareVaultActivation> hardwareVaultActivationsRepository;
        private IAsyncRepository<HardwareVaultProfile> hardwareVaultProfileRepository;
        private IAsyncRepository<LicenseOrder> licenseOrderRepository;
        private IAsyncRepository<HardwareVaultLicense> hardwareVaultLicenseRepository;
        private IAsyncRepository<AppSettings> appSettingsRepository;
        private IAsyncRepository<HardwareVaultTask> hardwareVaultTaskRepository;
        private IAsyncRepository<Account> accountRepository;
        private IAsyncRepository<Workstation> workstationsRepository;
        private IAsyncRepository<WorkstationProximityVault> workstationProximityVaultRepository;
        private IAsyncRepository<SoftwareVault> softwareVaultRepository;
        private IAsyncRepository<SoftwareVaultInvitation> softwareVaultInvitationRepository;
        private IAsyncRepository<SharedAccount> sharedAccountRepository;

        public IAccountService AccountService { get; set; }
        public IWorkstationService WorkstationService { get; set; }
        public IHardwareVaultTaskService HardwareVaultTaskService { get; set; }
        public IEmailSenderService EmailSenderService { get; set; }
        public ISoftwareVaultService SoftwareVaultService { get; set; }
        public IDataProtectionService DataProtectionService { get; set; }
        public ISharedAccountService SharedAccountService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        public ILicenseService LicenseService { get; set; }
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IEmployeeService EmployeeService { get; set; }

        public ServicesBuilder(string name)
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options;
            var dbContext = new ApplicationDbContext(dbOptions);

            employeeRepository = new Repository<Employee>(dbContext);
            hardwareVaultRepository = new Repository<HardwareVault>(dbContext);
            hardwareVaultActivationsRepository = new Repository<HardwareVaultActivation>(dbContext);
            hardwareVaultProfileRepository = new Repository<HardwareVaultProfile>(dbContext);
            licenseOrderRepository = new Repository<LicenseOrder>(dbContext);
            hardwareVaultLicenseRepository = new Repository<HardwareVaultLicense>(dbContext);
            appSettingsRepository = new Repository<AppSettings>(dbContext);
            hardwareVaultTaskRepository = new Repository<HardwareVaultTask>(dbContext);
            accountRepository = new Repository<Account>(dbContext);
            workstationsRepository = new Repository<Workstation>(dbContext);
            workstationProximityVaultRepository = new Repository<WorkstationProximityVault>(dbContext);
            softwareVaultRepository = new Repository<SoftwareVault>(dbContext);
            softwareVaultInvitationRepository = new Repository<SoftwareVaultInvitation>(dbContext);
            sharedAccountRepository = new Repository<SharedAccount>(dbContext);

            
            AccountService = new AccountService(accountRepository);
            WorkstationService = new WorkstationService(workstationsRepository, workstationProximityVaultRepository);
            HardwareVaultTaskService = new HardwareVaultTaskService(hardwareVaultTaskRepository);
            EmailSenderService = new EmailSenderService(null, null, null);
            SoftwareVaultService = new SoftwareVaultService(softwareVaultRepository, softwareVaultInvitationRepository, EmailSenderService);
            DataProtectionService = new DataProtectionService(null, null);
            SharedAccountService = new SharedAccountService(sharedAccountRepository, AccountService, HardwareVaultTaskService, DataProtectionService);
            AppSettingsService = new AppSettingsService(appSettingsRepository, DataProtectionService);
            LicenseService = new LicenseService(licenseOrderRepository, hardwareVaultLicenseRepository, hardwareVaultRepository, AppSettingsService, EmailSenderService, null, null);
            HardwareVaultService = new HardwareVaultService(hardwareVaultRepository, hardwareVaultActivationsRepository, hardwareVaultProfileRepository, LicenseService, HardwareVaultTaskService, AccountService, WorkstationService, AppSettingsService, DataProtectionService, null);
            EmployeeService = new EmployeeService(employeeRepository, HardwareVaultService, HardwareVaultTaskService, SoftwareVaultService, AccountService, SharedAccountService, WorkstationService, DataProtectionService);
        }
    }
}
