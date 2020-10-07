﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    public partial class AdministratorsPage : OwningComponentBase, IDisposable
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IEmailSenderService EmailSenderService { get; set; }
        public IMainTableService<ApplicationUser, ApplicationUserFilter> MainTableService { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<AdministratorsPage> Logger { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public AuthenticationState AuthenticationState { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                EmailSenderService = ScopedServices.GetRequiredService<IEmailSenderService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<ApplicationUser, ApplicationUserFilter>>();

                await InitializeHubAsync();
                AuthenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                await BreadcrumbsService.SetAdministrators();
                await MainTableService.InitializeAsync(ApplicationUserService.GetAdministratorsAsync, ApplicationUserService.GetAdministratorsCountAsync, ModalDialogService, StateHasChanged, nameof(ApplicationUser.Email), ListSortDirection.Ascending);

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task InviteAdminAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(InviteAdmin));
                builder.AddAttribute(1, nameof(InviteAdmin.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Invite Administrator", body, ModalDialogSize.Default);
        }

        private async Task ResendInviteAsync()
        {
            try
            {
                var callBakcUrl = await ApplicationUserService.GetCallBackUrl(MainTableService.SelectedEntity.Email, NavigationManager.BaseUri);
                await EmailSenderService.SendUserInvitationAsync(MainTableService.SelectedEntity.Email, callBakcUrl);
                await ToastService.ShowToastAsync("Administrator invited.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task DeleteAdminAsync()
        {
            if (MainTableService.Entities.Count == 1)
                return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAdministrator));
                builder.AddAttribute(1, nameof(DeleteAdministrator.ApplicationUserId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteAdministrator.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Administrator", body, ModalDialogSize.Default);
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Administrators, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
            });

            hubConnection.On(RefreshPage.AdministratorsUpdated, async () =>
            {
                await MainTableService.LoadTableDataAsync();
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}
