using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataLoader<T, TFilter>
    {
        Task<List<T>> GetEntitiesAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, TFilter filter);
        Task<int> GetEntitiesCountAsync(string searchText, TFilter filter);
    }
}
