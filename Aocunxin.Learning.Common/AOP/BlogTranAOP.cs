using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Castle.DynamicProxy;
using Aocunxin.Learning.IRepository.UnitOfWork;
using System.Linq;
using Aocunxin.Learning.Common.Attributes;
using System.Threading.Tasks;

namespace Aocunxin.Learning.Common.AOP
{
   public class BlogTranAOP: IInterceptor
    {
        private readonly IUnitOfWork _unitOfWork;

        public BlogTranAOP(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {

            //对当前方法的特性验证
            //如果需要验证
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            if (method.GetCustomAttributes(true).FirstOrDefault(x => x.GetType() == typeof(UseTranAttribute)) is UseTranAttribute)
            {
               try
                {
                    _unitOfWork.BeginTran();
                    invocation.Proceed();
                    if(IsAsyncMethod(invocation.Method))
                    {
                        if(invocation.Method.ReturnType==typeof(Task))
                        {
                            invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                              (Task)invocation.ReturnValue,
                              ex =>
                              {
                                  _unitOfWork.RollbackTran();

                              });
                        } else
                        {
                            invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                                invocation.Method.ReturnType.GenericTypeArguments[0],
                                invocation.ReturnValue,
                                ex =>
                                {
                                    _unitOfWork.RollbackTran();
                                }
                         );
                        }
                        _unitOfWork.CommitTran();
                    }
                  
                }
                catch (Exception)
                {
                    Console.WriteLine($"Rollback Transaction");
                    _unitOfWork.RollbackTran();
                }
            } else
            {

            }
        }

        public static bool IsAsyncMethod(MethodInfo method)
        {
            // 判断该值是否为泛型类型,返回一个表示可用于构造当前泛型类型的泛型类型定义的 对象
            return (method.ReturnType == typeof(Task) || (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));
        }

       
    }
}
