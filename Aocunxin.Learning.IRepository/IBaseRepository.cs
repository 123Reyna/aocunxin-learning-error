using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Aocunxin.Learning.IRepository
{
   public interface IBaseRepository<TEntity> where TEntity : class
    {

        Task<TEntity> QueryById(object objId);

        Task<int> Add(TEntity entity, Expression<Func<TEntity, object>> insertColumns = null);

        Task<bool> Update(TEntity entity);

        Task<bool> Update(TEntity entity, string strWhere);

        Task<bool> Update(string strSql, SugarParameter[] parameters = null);

        Task<bool> Update(object operateAnonymousObjects);

        bool IsExist(Expression<Func<TEntity, bool>> whereLambda);

        Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression);


        Task<List<T>> Query<T>(
            Expression<Func<TEntity, T>> selectExpression,
            Expression<Func<TEntity, bool>> whereLambda = null);
    }
}
