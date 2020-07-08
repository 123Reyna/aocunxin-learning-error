

using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using StackExchange.Profiling;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Blog.AOP
{
    /// <summary>
    /// 面向切面的缓存使用
    /// </summary>
    public class BlogLogAOP : IInterceptor
    {
        private readonly IHttpContextAccessor _accessor;

        public BlogLogAOP( IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public void Intercept(IInvocation invocation)
        {
            string UserName = _accessor.HttpContext.User.Identity.Name;
            //记录被拦截方法信息的日志信息
            var dataIntercept = "" +
                $"【当前操作用户】：{ UserName} \r\n" +
                $"【当前执行方法】：{ invocation.Method.Name} \r\n" +
                $"【携带的参数有】： {string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())} \r\n";
            try
            {
                MiniProfiler.Current.Step($"执行service方法:{invocation.Method.Name}()->");
                // 在被拦截的方法执行完毕后,继续执行当前方法,注意是被拦截的是异步的
                invocation.Proceed();
                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {

                    if(invocation.Method.ReturnType==typeof(Task))
                    {
                        invocation.ReturnValue=
                    }

                }
            }
        }

        public static bool IsAsyncMethod(System.Reflection.MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                );
        }
    }

   
}
