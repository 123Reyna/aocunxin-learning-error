using Aocunxin.Blog.Common.MemoryCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Blog.Extension
{
    public static class MemoryCacheSetup
    {
        public static void AddMemoryCacheSetup(this IServiceCollection services)
        {
            //AddTransient  请求获取-（GC回收-主动释放） 每一次获取的对象都不是同一个
            if (services == null) throw new ArgumentNullException(nameof(services));
            //AddScoped  请求开始-请求结束  在这次请求中获取的对象都是同一个 
            services.AddScoped<IMemoryCaching, MemoryCaching>();
          //  AddSingleton 项目启动 - 项目关闭   相当于静态类 只会有一个
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });
        }
    }
}
