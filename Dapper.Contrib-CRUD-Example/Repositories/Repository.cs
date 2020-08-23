using Dapper.Contrib.Extensions;
using Dapper.Contrib_CRUD_Example.IRepositores;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Contrib_CRUD_Example.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IDbConnection _dbConnection;

        #region Singleton yapı için
        public Repository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public virtual async Task InsertAsync(T entity)
        {
            await _dbConnection.InsertAsync(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            await _dbConnection.DeleteAsync(entity);
        }

        public virtual async Task DeleteAsync(object id)
        {
            var entity = await GetAsync(id);
            await DeleteAsync(entity);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbConnection.GetAllAsync<T>();
        }

        public virtual async Task<IEnumerable<T>> QueryAsync(string query, object parameters, CommandType commandType)
        {
            return await _dbConnection.QueryAsync<T>(query, parameters, null, null, commandType);
        }

        public virtual async Task<T> GetAsync(object id)
        {
            return await _dbConnection.GetAsync<T>(id);
        }

        public virtual async Task<T> QueryFirstOrDefaultAsync(string query, object parameters, CommandType commandType)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<T>(query, parameters, null, null, commandType);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            await _dbConnection.UpdateAsync(entity);
        }

        #endregion


        #region Scoped yapı için

        //public virtual async Task InsertAsync(T entity)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        await connection.InsertAsync(entity);
        //    }
        //}

        //public virtual async Task DeleteAsync(T entity)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        await connection.DeleteAsync(entity);
        //    }
        //}

        //public virtual async Task DeleteAsync(object id)
        //{
        //    var entity = await GetAsync(id);
        //    await DeleteAsync(entity);
        //}

        //public virtual async Task<IEnumerable<T>> GetAllAsync()
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        return await connection.GetAllAsync<T>();
        //    }
        //}

        //public virtual async Task<IEnumerable<T>> QueryAsync(string query, object parameters, CommandType commandType)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        return await connection.QueryAsync<T>(query, parameters, null, null, commandType);
        //    }
        //}

        //public virtual async Task<T> GetAsync(object id)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        return await connection.GetAsync<T>(id);
        //    }
        //}

        //public virtual async Task<T> QueryFirstOrDefaultAsync(string query, object parameters, CommandType commandType)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        return await connection.QueryFirstOrDefaultAsync<T>(query, parameters, null, null, commandType);
        //    }
        //}

        //public virtual async Task UpdateAsync(T entity)
        //{
        //    using (var connection = _dbConnection)
        //    {
        //        await connection.UpdateAsync(entity);
        //    }
        //}

        #endregion
    }
}
