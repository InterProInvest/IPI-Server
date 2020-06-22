using HES.Core.Entities;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ITemplateService
    {
        IQueryable<Template> Query();
        Task<Template> GetByIdAsync(dynamic id);
        Task<List<Template>> GetTemplatesAsync();
        Task<List<Template>> GetTemplatesAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions);
        Task<int> GetTemplatesCountAsync(DataLoadingOptions<TemplateFilter> dataLoadingOptions);
        Task<Template> CreateTmplateAsync(Template entity);
        Task EditTemplateAsync(Template template);
        Task DeleteTemplateAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Template, bool>> predicate);
    }
}