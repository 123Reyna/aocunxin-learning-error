using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Aocunxin.Learning.Common.Extensions;
using Aocunxin.Learning.Common;
using Aocunxin.Learning.Common.Attributes;
using Aocunxin.Learning.IRepository;
using Aocunxin.Learning.Repository;
using SqlSugar;
using Aocunxin.Learning.IRepository.UnitOfWork;
using Aocunxin.Learning.Repository.UnitOfWork;
using Aocunxin.Learning.Common.AOP;
using Google.Protobuf.WellKnownTypes;
using Aocunxin.Learning.AuthHelper;
using Aocunxin.Learning.Service;
using Aocunxin.Learning.IService;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using Aocunxin.Learning.AOther;

namespace Aocunxin.Learning
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            SysConfig.InitConfig();
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new SysConfig(Env.ContentRootPath));
            services.AddControllers(options=>
            {
                
            });
           
            services.AddMemoryCache();
            services.AddMiniProfilerSetup();
            services.AddSwaggerSetup();
            services.AddScoped<ISqlSugarClient> (tmp =>
            {
                // 连接
                BaseDbContext.SetConn();
                return BaseDbContext.Db as SqlSugarClient;
            });
            services.AddScoped<DBSeed>();
            services.AddScoped<IRoleModulePermissionServices, RoleModulePermissionServices>();
            //获取 Request/Headers 等信息
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthorizationSetup();

            // services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
           
            services.AddIocServiceScanner();

         
            //services.AddControllers().AddControllersAsServices();
            //services.AddSingleton<BlogTranAOP>().BuildServiceProvider().GetRequiredService<BlogTranAOP>();
        }

        //public void ConfigureContainer(ContainerBuilder builder)
        //{
        //    var cacheType = new List<Type>();
        //    builder.RegisterType<BlogTranAOP>();
        //    cacheType.Add(typeof(BlogTranAOP));
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
           
            app.UseHttpsRedirection();
            app.UseRouting();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();
          
            app.UseMiniProfiler();
            app.AddSwaggerSetupApp();
           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
