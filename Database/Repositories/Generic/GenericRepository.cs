using System.Linq.Expressions;
using Database.Context;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories.Generic;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
	internal ApplicationDbContext context;
	internal DbSet<T> dbSet;

	public GenericRepository(ApplicationDbContext context)
	{
		this.context = context;
		dbSet = context.Set<T>();
	}

	public virtual async Task<IEnumerable<T>> Get(
		Expression<Func<T, bool>>? filter = null,
		Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
		string includeProperties = "",
		bool track = false)
	{
		// Create IQuerayble
		IQueryable<T> query = dbSet;

		// Add filter to query
		if (filter != null)
		{
			query = query.Where(filter);
		}

		// Include properties for relationship loading
		// ReSharper disable once LoopCanBeConvertedToQuery
		foreach (string includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
		{
			query = query.Include(includeProperty.Trim());
		}

		if (!track)
		{
			query.AsNoTracking();
		}

		// Order results and execute request
		return orderBy == null ? await query.ToListAsync() : await orderBy(query).ToListAsync();
	}

	public virtual async Task<T?> GetByID(object id)
	{
		// Find result matching the id to the primary key of the object
		return await dbSet.FindAsync(id).ConfigureAwait(false);
	}

	public virtual async Task<T> Add(T entity)
	{
		// Add object to database
		await dbSet.AddAsync(entity).ConfigureAwait(false);

		// Save changes
		context.SaveChanges();

		// Return new database object
		return entity;
	}

	public virtual async Task BulkAdd(List<T> entities, BulkConfig? config = null)
	{
		await context.BulkInsertAsync(entities, config).ConfigureAwait(false);
	}

	public virtual async Task Delete(object id)
	{
		// Found object using ID
		T? entityToDelete = await dbSet.FindAsync(id).ConfigureAwait(false);

		if (entityToDelete is not null)
		{
			// Delete object
			Delete(entityToDelete);
		}
	}

	public virtual void Delete(T entityToDelete)
	{
		// Make sure that the object is being tracked by EF
		// learn more about EF entity states here - https://docs.microsoft.com/en-us/ef/ef6/saving/change-tracking/entity-state
		if (context.Entry(entityToDelete).State == EntityState.Detached)
		{
			dbSet.Attach(entityToDelete);
		}

		// Changes the state of object to Deleted
		dbSet.Remove(entityToDelete);

		// Update changes to database
		context.SaveChanges();
	}

	public virtual async Task DeleteFromQuery(Expression<Func<T, bool>> query, int batchSize = 10000)
	{
		int rowsAffected;
		do
		{
			rowsAffected = await dbSet.Where(query).Take(batchSize).BatchDeleteAsync().ConfigureAwait(false);
		} while (rowsAffected >= batchSize);
	}

	public virtual void Update(T entityToUpdate)
	{
		// Make sure the object is being tracked by EF
		dbSet.Attach(entityToUpdate);

		// Set the state to modified
		context.Entry(entityToUpdate).State = EntityState.Modified;

		// Update changes to database
		context.SaveChanges();
	}

	public virtual async Task UpdateFromQuery(Expression<Func<T, bool>> query, Expression<Func<T, T>> updateExpression)
	{
		await dbSet.Where(query).BatchUpdateAsync(updateExpression).ConfigureAwait(false);
	}

	public IQueryable<T> Where(Expression<Func<T, bool>>? filter = null, bool track = false)
	{
		// Create IQuerayble
		IQueryable<T> query = dbSet;

		if (!track)
		{
			query.AsNoTracking();
		}

		return filter == null ? query : query.Where(filter);
	}
}