using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAsyncRepository<T> where T : class
    {
        IQueryable<T> Query();
        IQueryable<T> SqlQuery(string sql);
        Task<T> GetByIdAsync(string id);
        Task<T> GetByCompositeKeyAsync(object[] obj);
        Task<T> AddAsync(T entity);
        Task<IList<T>> AddRangeAsync(IList<T> entity);
        Task<T> UpdateAsync(T entity);
        Task<IList<T>> UpdatRangeAsync(IList<T> entity);
        Task UpdateOnlyPropAsync(T entity, string[] properties);
        Task UpdateOnlyPropAsync(IList<T> entity, string[] properties);
        Task Unchanged(T entity);
        Task<T> DeleteAsync(T entity);
        Task DeleteRangeAsync(IList<T> entity);
        Task<bool> ExistAsync(Expression<Func<T, bool>> predicate);
    }
}