using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplatesPage : ComponentBase
    {
        [Inject] public ITemplateService TemplateService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IMainTableService<Template, TemplateFilter> MainTableService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await MainTableService.InitializeAsync(TemplateService.GetTemplatesAsync, TemplateService.GetTemplatesCountAsync, StateHasChanged, nameof(Template.Name), ListSortDirection.Descending);
            await BreadcrumbsService.SetTemplates();
        }

        private async Task CreateTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateTemplate));
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Template", body, ModalDialogSize.Default);
        }
    }
}
