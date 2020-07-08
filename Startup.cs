using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using aocunxin.Blog.AOP;
using Aocunxin.Blog.Common.DB;
using Aocunxin.Blog.Common.Helper;
using Aocunxin.Blog.Common.LogHelper;
using Aocunxin.Blog.Common.Redis;
using Aocunxin.Blog.Core;
using Aocunxin.Blog.Core.Convert;
using Aocunxin.Blog.Extension;
using Aocunxin.Blog.Extensions;
using Aocunxin.Blog.Filter;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;
using static Aocunxin.Blog.SwaggerHelper.CustomApiVersion;

namespace aocunxin.Blog
{
    public class Startup
    {
        public string VersionApiName = "V1";
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

      
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRedisCacheManager, RedisCacheManager>();
            services.AddSingleton(new Appsettings(Env.ContentRootPath));
            services.AddSingleton(new LogLock(Env.ContentRootPath));
          

            services.AddMemoryCacheSetup();
            services.AddSqlsugarSetup();
            services.AddDbSetup();
         
            services.AddSwaggerSetup();
          
            services.AddAuthorizationSetup();

          //  services.AddScoped<UseServiceDIAttribute>();

            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelAttribute>();
                options.Filters.Add<ApiResultFilterAttribute>();
            })
            .AddJsonOptions(options =>
            {
                // �������ʱʱ��ת������
                options.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());

            });
            // ���ÿ���������������Դ
            services.AddCors(options =>
            {
                options.AddPolicy("MyAllowSpecificOrigins",

                     builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    );

            });

            
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;
            //builder.RegisterType<AdvertisementServices>().As<IAdvertisementServices>();


            #region ���нӿڲ�ķ���ע��

            var servicesDllFile = Path.Combine(basePath, "Aocunxin.Blog.Service.dll");
            var repositoryDllFile = Path.Combine(basePath, "Aocunxin.Blog.Repository.dll");


            // AOP ���أ������Ҫ��ָ���Ĺ��ܣ�ֻ��Ҫ�� appsettigns.json ��Ӧ��Ӧ true ���С�
            var cacheType = new List<Type>();
            if (Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "Enabled" }).ObjToBool())
            {
                //builder.RegisterType<BlogRedisCacheAOP>();
                //cacheType.Add(typeof(BlogRedisCacheAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "MemoryCachingAOP", "Enabled" }).ObjToBool())
            {
                //builder.RegisterType<BlogCacheAOP>();
                //cacheType.Add(typeof(BlogCacheAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "TranAOP", "Enabled" }).ObjToBool())
            {
                builder.RegisterType<BlogTranAOP>();
                cacheType.Add(typeof(BlogTranAOP));
            }
            if (Appsettings.app(new string[] { "AppSettings", "LogAOP", "Enabled" }).ObjToBool())
            {
                //builder.RegisterType<BlogLogAOP>();
                //cacheType.Add(typeof(BlogLogAOP));
            }

            // ��ȡ Repository.dll ���򼯷��񣬲�ע��
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                   .AsImplementedInterfaces()
                   .InstancePerDependency();


            // ��ȡ Service.dll ���򼯷��񣬲�ע��
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                      .AsImplementedInterfaces()
                      .InstancePerDependency()
                      .EnableInterfaceInterceptors()//����Autofac.Extras.DynamicProxy;
                      .InterceptedBy(cacheType.ToArray());//����������������б�����ע�ᡣ


            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // ʹ��cookie
            app.UseCookiePolicy();

            app.UseSwagger();
            // app.UseSwaggerUI(v=> v.SwaggerEndpoint($"/{curAppName}/swagger/{VersionApiName}/swagger.json", $"{VersionApiName} doc"));

            app.UseSwaggerUI(c =>
            {
                //���ݰ汾���Ƶ��� ����չʾ
                var ApiName = Appsettings.app(new string[] { "Startup", "ApiName" });
                typeof(ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(version =>
                {
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{ApiName} {version}");
                });
                // ��swagger��ҳ�����ó������Զ����ҳ�棬�ǵ�����ַ�����д�������������.index.html���������Ҽ����ԣ�Ƕ�����Դ
                //c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Aocunxin.Blog.xml");//���������MiniProfiler�������ܼ�صģ������£���������AOP�Ľӿ����ܷ�����������㲻��Ҫ��������ʱ��ע�͵�����Ӱ���֡�
                c.RoutePrefix = ""; //·�����ã�����Ϊ�գ���ʾֱ���ڸ�������localhost:8001�����ʸ��ļ�,ע��localhost:8001/swagger�Ƿ��ʲ����ģ�ȥlaunchSettings.json��launchUrlȥ����������뻻һ��·����ֱ��д���ּ��ɣ�����ֱ��дc.RoutePrefix = "doc";
            });



            app.UseRouting();

            // �ȿ�����֤
            app.UseAuthentication();
            // Ȼ������Ȩ�м��
            app.UseAuthorization();

            app.UseAuthorization();

            app.UseMiniProfiler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
