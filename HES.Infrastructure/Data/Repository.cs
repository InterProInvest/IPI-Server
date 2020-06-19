using Dasync.Collections;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Infrastructure
{
    public class Repository<T> : IAsyncRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query()
        {
            return _context.Set<T>().AsQueryable();
        }

        public IQueryable<T> SqlQuery(string sql)
        {
            return _context.Set<T>().FromSqlRaw(sql);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> GetByCompositeKeyAsync(object[] obj)
        {
            return await _context.Set<T>().FindAsync(obj);
        }

        public async Task<T> AddAsync(T entity)
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<IList<T>> AddRangeAsync(IList<T> entity)
        {
            _context.Set<T>().AddRange(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<IList<T>> UpdatRangeAsync(IList<T> entity)
        {
            _context.Set<T>().UpdateRange(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateOnlyPropAsync(T entity, string[] properties)
        {
            foreach (var prop in properties)
            {
                _context.Entry(entity).Property(prop).IsModified = true;
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOnlyPropAsync(IList<T> entity, string[] properties)
        {
            foreach (var item in entity)
            {
                foreach (var prop in properties)
                {
                    _context.Entry(item).Property(prop).IsModified = true;
                }
            }

            _context.UpdateRange(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<T> DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteRangeAsync(IList<T> entity)
        {
            _context.Set<T>().RemoveRange(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).AnyAsync();
        }

        public Task UnchangedAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Unchanged;
            return Task.CompletedTask;
        }

        public Task DetachedAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;

            foreach (var collectionEntry in _context.Entry(entity).Collections)
            {
                if (collectionEntry.CurrentValue != null)
                {
                    foreach (var current in collectionEntry.CurrentValue)
                        collectionEntry.EntityEntry.Context.Entry(current).State = EntityState.Detached;
                }
            }

            return Task.CompletedTask;
        }

        public async Task ReloadAsync(T entity)
        {
            await _context.Entry(entity).ReloadAsync();
        }
    }
}