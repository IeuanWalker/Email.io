using App.Database.Context;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace App.Database.Repositories.Generic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        public GenericRepository(ApplicationDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        internal ApplicationDbContext Context;
        internal DbSet<T> DbSet;

        public virtual async Task<IEnumerable<T>> Get(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "")
        {
            // Create IQuerayble
            IQueryable<T> query = DbSet;

            // Add filter to query
            if (filter != null) query = query.Where(filter);

            // Include properties for relationship loading
            foreach (string includeProperty in includeProperties.Split(new[] { ',' },
                StringSplitOptions.RemoveEmptyEntries)) query = query.Include(includeProperty.Trim());

            // Order results and execute request
            return orderBy == null ? await query.ToListAsync().ConfigureAwait(false) : await orderBy(query).ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<T?> GetById(object id) => await DbSet.FindAsync(id).ConfigureAwait(false);

        public virtual async Task<T> Add(T entity)
        {
            // Add object to database
            await DbSet.AddAsync(entity).ConfigureAwait(false);

            // Save changes
            Context.SaveChanges();

            // Return new database object
            return entity;
        }

        public virtual async Task BulkAdd(List<T> entities, BulkConfig? config = null)
        {
            await Context.BulkInsertAsync(entities, config).ConfigureAwait(false);
        }

        public virtual async Task Delete(object id)
        {
            // Found object using ID
            T? entityToDelete = await DbSet.FindAsync(id).ConfigureAwait(false);

            // Delete object
            if (entityToDelete is not null)
            {
                Delete(entityToDelete);
            }
        }

        public virtual void Delete(T entityToDelete)
        {
            // Make sure that the object is being tracked by EF
            // learn more about EF entity states here - https://docs.microsoft.com/en-us/ef/ef6/saving/change-tracking/entity-state
            if (Context.Entry(entityToDelete).State == EntityState.Detached) DbSet.Attach(entityToDelete);

            // Changes the state of object to Deleted
            DbSet.Remove(entityToDelete);

            // Update changes to database
            Context.SaveChanges();
        }

        public virtual async Task DeleteFromQuery(Expression<Func<T, bool>> query)
        {
            await DbSet.Where(query).BatchDeleteAsync().ConfigureAwait(false);
        }

        public virtual void Update(T entityToUpdate)
        {
            // Make sure the object is being tracked by EF
            DbSet.Attach(entityToUpdate);

            // Set the state to modified
            Context.Entry(entityToUpdate).State = EntityState.Modified;

            // Update changes to database
            Context.SaveChanges();
        }

        public virtual async Task UpdateFromQuery(Expression<Func<T, bool>> query, Expression<Func<T, T>> updateExpression)
        {
            await DbSet.Where(query).BatchUpdateAsync(updateExpression).ConfigureAwait(false);
        }

        public IQueryable<T> Query(Expression<Func<T, bool>>? query = null)
        {
            return query == null ? DbSet : DbSet.Where(query);
        }
    }
}