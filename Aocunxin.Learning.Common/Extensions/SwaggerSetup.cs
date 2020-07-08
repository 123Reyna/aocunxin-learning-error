
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Linq;
using static Aocunxin.Learning.Common.Extensions.CustomApiVersion;

namespace Aocunxin.Learning.Common.Extensions
{

   
    public static   class SwaggerSetup
    {

        public static void AddSwaggerSetup(this IServiceCollection service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));

            var basePath = AppContext.BaseDirectory;
            var apiName = SysConfig.app(new string[] { "Startup", "ApiName" });

            service.AddSwaggerGen(c =>
            {
                
                
                //遍历出全部的版本，做文档信息展示
                typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Version = version,
                        Title = $"{apiName} 接口文档——Netcore 3.1",
                        Description = $"{apiName} HTTP API " + version,
                        //Contact = new OpenApiContact { Name = apiName, Email = "", Url = new Uri("") },
                        //License = new OpenApiLicense { Name = apiName + " 官方文档", Url = new Uri("") }
                    });

                    c.OrderActionsBy(o => o.RelativePath);
                });
             

                try
                {
                    //就是这里
                    var xmlPath = Path.Combine(basePath, "Aocunxin.Learning.xml");//这个就是刚刚配置的xml文件名
                    c.IncludeXmlComments(xmlPath, true);//默认的第二个参数是false，这个是controller的注释，记得修改

                    //var xmlModelPath = Path.Combine(basePath, "Blog.Core.Model.xml");//这个就是Model层的xml文件名
                    //c.IncludeXmlComments(xmlModelPath);
                }
                catch (Exception ex)
                {
                   // log.Error("Blog.Core.xml和Blog.Core.Model.xml 丢失，请检查并拷贝。\n" + ex.Message);
                }

                // 开启加权小锁
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // 在header中添加token，传递到后台
                c.OperationFilter<SecurityRequirementsOperationFilter>();


                // 必须是 oauth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });
        }

        public static void AddSwaggerSetupApp(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                //根据版本名称倒序 遍历展示
                var ApiName = SysConfig.app(new string[] { "Startup", "ApiName" });
                typeof(ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(version =>
                {
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{ApiName} {version}");
                });
                //设置为 - 1 可不显示models
                c.DefaultModelsExpandDepth(-1); 
                 // 将swagger首页，设置成我们自定义的页面，记得这个字符串的写法：解决方案名.index.html，并且是右键属性，嵌入的资源
                // c.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Blog.Core.index.html");//这里是配合MiniProfiler进行性能监控的，《文章：完美基于AOP的接口性能分析》，如果你不需要，可以暂时先注释掉，不影响大局。
                c.RoutePrefix = ""; //路径配置，设置为空，表示直接在根域名（localhost:8001）访问该文件,注意localhost:8001/swagger是访问不到的，去launchSettings.json把launchUrl去掉，如果你想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "doc";
            });
        }
    }


    public class CustomApiVersion
    {
        /// <summary>
        /// Api接口版本 自定义
        /// </summary>
        public enum ApiVersions
        {
            /// <summary>
            /// V1 版本
            /// </summary>
            V1 = 1,
            /// <summary>
            /// V2 版本
            /// </summary>
            V2 = 2,
        }
    }
}
