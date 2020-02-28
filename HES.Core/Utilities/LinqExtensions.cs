using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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

        public static async Task<int> SearchCountAsync<T>(this IQueryable<T> query, string search = null)
        {
            return await Task.Run(async () =>
            {
                if (string.IsNullOrWhiteSpace(search))
                {
                    return await query.CountAsync();
                }

                search = search.Trim().ToLower();
                int count = 0;

                foreach (var item in query)
                {
                    foreach (var property in item.GetType().GetProperties().Where(p => p.Name.ToLower() == "name" || p.Name.ToLower() == "email" || p.Name.ToLower() == "description"))
                    {
                        var propValue = property.GetValue(item)?.ToString();

                        if (propValue == null)
                            continue;

                        var isContains = propValue.ToLower().Contains(search);
                        if (isContains)
                        {
                            count++;
                            break;
                        }
                    }
                }

                return count;
            });
        }

        //public static IQueryable<T> WhereSearch<T>(this IQueryable<T> query, string search = null)
        //{
        //    //if (string.IsNullOrWhiteSpace(search))
        //    //    return query;

        //    //query = query.Where(x => );

        //    //var a = await query.ToListAsync();

        //    //return query;

        //    //if (string.IsNullOrWhiteSpace(search))
        //    //    return query;

        //    //search = search.Trim().ToLower();
        //    //var resultList = new List<T>();

        //    //foreach (var item in query)
        //    //{
        //    //    foreach (var property in item.GetType().GetProperties())
        //    //    {
        //    //        var propValue = property.GetValue(item)?.ToString();

        //    //        if (propValue == null)
        //    //            continue;

        //    //        var isContains = propValue.ToLower().Contains(search);
        //    //        if (isContains)
        //    //            resultList.Add(item);
        //    //    }
        //    //}

        //    //return resultList.AsQueryable();
        //}
    }
}
