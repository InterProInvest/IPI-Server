﻿using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class Parameters : ComponentBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public ILogger<Parameters> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }

        private LicensingSettings _licensing;
        //private EmailSettings _email;
        //private ServerSettings _server;
        private DomainHost _domain;
        private bool _licensingIsBusy;
        //private bool _emailIsBusy;
        //private bool _serverIsBusy;
        private bool _initialized;

        class DomainHost
        {
            [Required]
            public string Host { get; set; }
        }

        protected override async Task OnInitializedAsync()
        {
            await BreadcrumbsService.SetParameters();
            _licensing = await LoadLicensingSettingsAsync();
            //_email = await LoadEmailSettingsAsync();
            //_server = await LoadServerSettingsAsync();
            _domain = await LoadDomainSettingsAsync();
            _initialized = true;
        }

        private async Task<LicensingSettings> LoadLicensingSettingsAsync()
        {
            var licensingSettings = await AppSettingsService.GetLicensingSettingsAsync();

            if (licensingSettings == null)
                return new LicensingSettings();

            return licensingSettings;
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                if (_licensingIsBusy)
                    return;

                _licensingIsBusy = true;
                await AppSettingsService.SetLicensingSettingsAsync(_licensing);
                ToastService.ShowToast("License settings updated.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                _licensingIsBusy = false;
            }
        }

        //private async Task<EmailSettings> LoadEmailSettingsAsync()
        //{
        //    var sttings = await AppSettingsService.GetEmailSettingsAsync();

        //    if (sttings == null)
        //        return new EmailSettings();

        //    return sttings;
        //}

        //private async Task UpdateEmailSettingsAsync()
        //{
        //    try
        //    {
        //        if (_emailIsBusy)
        //            return;

        //        _emailIsBusy = true;

        //        await AppSettingsService.SetEmailSettingsAsync(_email);
        //        ToastService.ShowToast("Email settings updated.", ToastLevel.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex.Message);
        //        ToastService.ShowToast(ex.Message, ToastLevel.Error);
        //    }
        //    finally
        //    {
        //        _emailIsBusy = false;
        //    }
        //}

        //private async Task<ServerSettings> LoadServerSettingsAsync()
        //{
        //    var serverSettings = await AppSettingsService.GetServerSettingsAsync();

        //    if (serverSettings == null)
        //        return new ServerSettings();

        //    return serverSettings;
        //}

        //private async Task UpdateServerSettingsAsync()
        //{
        //    try
        //    {
        //        if (_serverIsBusy)
        //        {
        //            return;
        //        }

        //        _serverIsBusy = true;
        //        await AppSettingsService.SetServerSettingsAsync(_server);
        //        ToastService.ShowToast("Server settings updated.", ToastLevel.Success);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex.Message);
        //        ToastService.ShowToast(ex.Message, ToastLevel.Error);
        //    }
        //    finally
        //    {
        //        _serverIsBusy = false;
        //    }
        //}

        private async Task<DomainHost> LoadDomainSettingsAsync()
        {
            var domainSettings = await AppSettingsService.GetLdapSettingsAsync();

            if (domainSettings == null)
                return new DomainHost();

            return new DomainHost() { Host = domainSettings.Host };
        }

        private async Task UpdateDomainSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(LdapCredentials));
                builder.AddAttribute(1, "Host", _domain.Host);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Active Directory", body);
        }
    }
}