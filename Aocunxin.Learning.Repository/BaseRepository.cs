using Aocunxin.Learning.IRepository;
using Aocunxin.Learning.IRepository.UnitOfWork;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Aocunxin.Learning.Repository
{
    /// 型参数约束,T class必须是一个类 new() T必须要有一个无参构造函数
    public class BaseRepository<TEntity>: IBaseRepository<TEntity> where TEntity : class, new()
    {

        private readonly IUnitOfWork _unitOfWork;
        private SqlSugarClient _dbBase;

        //public BaseRepository()
        //{
            
        //}

        //public BaseRepository(ISqlSugarClient db)
        //{
        //    _dbBase = db as SqlSugarClient;
        //}

        public BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _dbBase = unitOfWork.GetDbClient();
        }

        protected ISqlSugarClient Db
        {
            get { return _dbBase; }
        }


        /// <summary>
        ///  初始化
        /// </summary>
        public SqlSugarClient Instance()
        {
            BaseDbContext.SetConn();
           return  _dbBase = BaseDbContext.Db as SqlSugarClient;
        }

        /// <summary>
        ///  初始化
        /// </summary>
        /// <param name="listKey">数据连接Key</param> 
        public SqlSugarClient Instance(List<string> listKey)
        {
            BaseDbContext.SetConn(listKey);
           return _dbBase = BaseDbContext.Db as SqlSugarClient;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="serverIp">服务器IP</param>
        /// <param name="user">用户名</param>
        /// <param name="pass">密码</param>
        /// <param name="dataBase">数据库</param>
        /// <returns>值</returns>
        public SqlSugarClient Instance(string serverIp, string user, string pass, string dataBase)
        {
            BaseDbContext.SetConn(serverIp, user, pass, dataBase);
           return _dbBase = BaseDbContext.Db as SqlSugarClient;
        }


        public async Task<TEntity> QueryById(object objId)
        {
            //return await Task.Run(() => _db.Queryable<TEntity>().InSingle(objId));
            return await _dbBase.Queryable<TEntity>().In(objId).SingleAsync();
        }


        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await _dbBase.Queryable<TEntity>().WhereIF(whereExpression != null, whereExpression).ToListAsync();
        }

        public async Task<List<T>> Query<T>(
            Expression<Func<TEntity,  T>> selectExpression,
            Expression<Func<TEntity, bool>> whereLambda = null) 
        {
            if (whereLambda == null)
            {
                return await _dbBase.Queryable<TEntity>().Select(selectExpression).ToListAsync();
            }
            return await _dbBase.Queryable<TEntity>().Where(whereLambda).Select(selectExpression).ToListAsync();
        }

        /// <summary>
        /// 写入实体数据
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <param name="insertColumns">指定只插入列</param>
        /// <returns>返回自增量列</returns>
        public async Task<int> Add(TEntity entity, Expression<Func<TEntity, object>> insertColumns = null)
        {
            var insert = _dbBase.Insertable(entity);
            if (insertColumns == null)
            {
                return await insert.ExecuteReturnIdentityAsync();
            }
            else
            {
                return await insert.InsertColumns(insertColumns).ExecuteReturnIdentityAsync();
            }
        }

     
        public async Task<bool> Update(TEntity entity)
        {
            //这种方式会以主键为条件
            return await _dbBase.Updateable(entity).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> Update(TEntity entity, string strWhere)
        {
            return await _dbBase.Updateable(entity).Where(strWhere).ExecuteCommandHasChangeAsync();
        }

        public async Task<bool> Update(string strSql, SugarParameter[] parameters = null)
        {
            return await _dbBase.Ado.ExecuteCommandAsync(strSql, parameters) > 0;
        }

        public async Task<bool> Update(object operateAnonymousObjects)
        {
            return await _dbBase.Updateable<TEntity>(operateAnonymousObjects).ExecuteCommandAsync() > 0;
        }


        public  bool IsExist(Expression<Func<TEntity, bool>> whereLambda)
        {
            return  _dbBase.Queryable<TEntity>().Any(whereLambda);
        }


        public async Task<List<TResult>> QueryMuch<T, T2, T3, TResult>(
           Expression<Func<T, T2, T3, object[]>> joinExpression,
           Expression<Func<T, T2, T3, TResult>> selectExpression,
           Expression<Func<T, T2, T3, bool>> whereLambda = null) where T : class, new()
        {
            if (whereLambda == null)
            {
                return await _dbBase.Queryable(joinExpression).Select(selectExpression).ToListAsync();
            }
            return await _dbBase.Queryable(joinExpression).Where(whereLambda).Select(selectExpression).ToListAsync();
        }


    }
}
