using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Database.Repositories.Generic;

/// <summary>
/// Used as a Generic EF repository that code do all CRUD functions and queries
/// </summary>
/// <typeparam name="T">Is the database object use (i.e. table)</typeparam>
public interface IGenericRepository<T> where T : class
{
	/// <summary>
	/// Get data from the database using the different options
	/// </summary>
	/// <param name="filter">This is a simple LINQ query to query the database</param>
	/// <param name="orderBy">Order of the data</param>
	/// <param name="includeProperties">Provide a comma-delimited list of navigation properties for eager loading</param>
	/// <returns>List of <c>T</c></returns>
	/// <example>
	/// Filter: <c> x => x.FirstName == "Ieuan" </c>
	/// OrderBy: <c> x => x.OrderByDescending(a => a.Id) </c>
	/// </example>
	Task<IEnumerable<T>> Get(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string includeProperties = "", bool track = false);

	/// <summary>
	/// Get object using ID (primary key).
	/// </summary>
	/// <param name="id">Primary key value of object</param>
	/// <returns>Returns object if available</returns>
	/// <remarks>
	/// This is based on the primary key, meaning that its possible to search for any type of ids - i.e. int, Guids, string etc.
	/// </remarks>
	Task<T?> GetByID(object id);

	/// <summary>
	/// Add object to database
	/// </summary>
	/// <param name="entity"></param>
	/// <returns>Returns object including the new ID</returns>
	Task<T> Add(T entity);

	/// <summary>
	/// Delete object using the ID (primary key)
	/// </summary>
	/// <param name="id">Primary key value of object</param>
	/// <remarks>
	/// This is based on the primary key, meaning that its possible to search for any type of ids - i.e. int, Guids, string etc.
	/// </remarks>
	Task Delete(object id);

	/// <summary>
	/// Delete object using object
	/// </summary>
	/// <param name="entityToDelete"></param>
	void Delete(T entityToDelete);

	/// <summary>
	/// Delete rows using query
	/// </summary>
	/// <param name="query"></param>
	Task DeleteFromQuery(Expression<Func<T, bool>> query, int batchSize = 10000);

	/// <summary>
	/// Update object
	/// </summary>
	/// <param name="entityToUpdate"></param>
	Task Update(T entityToUpdate);

	/// <summary>
	/// Update rows using query
	/// </summary>
	/// <param name="query"></param>
	/// <param name="updateExpression"></param>
	Task UpdateFromQuery(Expression<Func<T, bool>> query, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls, int? batchSize = null);

	/// <summary>
	/// Enable to generate specific queries
	/// </summary>
	IQueryable<T> Where(Expression<Func<T, bool>>? filter = null, bool track = false);
}