using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.SoftwareVault;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SoftwareVaultService : ISoftwareVaultService
    {
        private readonly IAsyncRepository<SoftwareVault> _softwareVaultRepository;
        private readonly IAsyncRepository<SoftwareVaultInvitation> _softwareVaultInvitationRepository;
        private readonly IEmailSenderService _emailSenderService;

        public SoftwareVaultService(IAsyncRepository<SoftwareVault> softwareVaultRepository,
                                    IAsyncRepository<SoftwareVaultInvitation> softwareVaultInvitationRepository,
                                    IEmailSenderService emailSenderService)
        {
            _softwareVaultRepository = softwareVaultRepository;
            _softwareVaultInvitationRepository = softwareVaultInvitationRepository;
            _emailSenderService = emailSenderService;
        }

        public async Task<List<SoftwareVault>> GetSoftwareVaultsAsync()
        {
            return await _softwareVaultRepository
               .Query()
               .Include(x => x.Employee)
               .AsTracking()
               .ToListAsync();
        }

        public async Task<List<SoftwareVaultInvitation>> GetSoftwareVaultInvitationsAsync()
        {
            return await _softwareVaultInvitationRepository
               .Query()
               .Include(x => x.Employee)
               .AsTracking()
               .ToListAsync();
        }

        public async Task CreateAndSendInvitationAsync(Employee employee, Server server, DateTime validTo)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.Id == null)
                throw new ArgumentNullException(nameof(employee.Id));

            if (employee.Email == null)
                throw new ArgumentNullException(nameof(employee.Email));

            var activationCode = GenerateActivationCode();

            var invitation = new SoftwareVaultInvitation()
            {
                EmployeeId = employee.Id,
                Status = InviteVaultStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ValidTo = validTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AcceptedAt = null,
                SoftwareVaultId = null,
                ActivationCode = activationCode
            };

            var created = await _softwareVaultInvitationRepository.AddAsync(invitation);

            var activation = new SoftwareVaultActivation()
            {
                ServerAddress = server.Url,
                ActivationId = created.Id,
                ActivationCode = activationCode
            };

            await _emailSenderService.SendSoftwareVaultInvitationAsync(employee, activation, validTo);
        }

        private int GenerateActivationCode()
        {
            return new Random().Next(100000, 999999);
        }
    }
}