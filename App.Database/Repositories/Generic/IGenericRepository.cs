using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace App.Database.Repositories.Generic
{
    /// <summary>
    ///     Used as a Generic EF repository that code do all CRUD functions and queries
    /// </summary>
    /// <typeparam name="T">Is the database object use (i.e. table)</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        ///     Get data from the database using the different options
        /// </summary>
        /// <param name="filter">This is a simple linq query to query the database</param>
        /// <param name="orderBy">Order of the data</param>
        /// <param name="includeProperties">Provide a comma-delimited list of navigation properties for eager loading</param>
        /// <returns>List of <c>T</c></returns>
        /// <example>
        ///     Filter: <code> x => x.FirstName == "Ieuan" </code>
        ///     OrderBy: <code> x => x.OrderByDescending(a => a.Id) </code>
        /// </example>
        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "");

        /// <summary>
        ///     Get object using ID (primary key).
        /// </summary>
        /// <param name="id">Primary key value of object</param>
        /// <returns>Returns object if available</returns>
        /// <remarks>
        ///     This is based on the primary key, meaning that its possible to search for any type of ids - i.e. int, Guids, string
        ///     etc.
        /// </remarks>
        Task<T> GetById(object id);

        /// <summary>
        ///     Add object to database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Returns object including the new ID</returns>
        Task<T> Add(T entity);

        /// <summary>
        ///     Delete object using the ID (primary key)
        /// </summary>
        /// <param name="id">Primary key value of object</param>
        /// <returns></returns>
        /// <remarks>
        ///     This is based on the primary key, meaning that its possible to search for any type of ids - i.e. int, Guids, string
        ///     etc.
        /// </remarks>
        Task Delete(object id);

        /// <summary>
        ///     Delete object using object
        /// </summary>
        /// <param name="entityToDelete"></param>
        void Delete(T entityToDelete);

        /// <summary>
        ///     Update object
        /// </summary>
        /// <param name="entityToUpdate"></param>
        void Update(T entityToUpdate);

        /// <summary>
        /// Enable to generate specific queries
        /// </summary>
        IQueryable<T> Query();
    }
}
