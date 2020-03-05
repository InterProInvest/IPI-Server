using System;
using System.Linq;
using System.Linq.Expressions;

namespace HES.Core.Utilities
{
    public static class LinqExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string sortColumn, bool descending = false)
        {
            const string OrderBy = "OrderBy";
            const string OrderByDesc = "OrderByDescending";

            var parameter = Expression.Parameter(typeof(T));

            string command = !descending ? OrderBy : OrderByDesc;

            if (string.IsNullOrWhiteSpace(sortColumn))
                throw new ArgumentNullException(nameof(sortColumn));

            var property = typeof(T).GetProperty(sortColumn);

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType },
               query.Expression, Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
