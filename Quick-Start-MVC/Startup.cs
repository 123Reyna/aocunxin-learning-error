using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Quick_Start_MVC
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
                
            });
            var builder = services.AddIdentityServer()
               .AddInMemoryApiScopes(Config.ApiScopes)
               .AddInMemoryClients(Config.Clients)
               .AddTestUsers(TestUsers.Users)
               //与 OAuth 相比，OIDC(OpenID Connect) 中的 scopes 不仅代表 API，还代表了诸如 用户id、用户名 或 邮箱地址等身份数据。
               .AddInMemoryIdentityResources(Config.IdentityResources);

            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                    .AddGoogle("Google", options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = "<insert here>";
                        options.ClientSecret = "<insert here>";
                    })
                    // 用了认证
                    .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
                     {
                         options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                         options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                         options.Authority = "https://demo.identityserver.io/";
                         options.ClientId = "interactive.confidential";
                         options.ClientSecret = "secret";
                         options.ResponseType = "code";

                         options.TokenValidationParameters = new TokenValidationParameters
                         {
                             NameClaimType = "name",
                             RoleClaimType = "role"
                         };
                     });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //启用IdentityServer
            app.UseIdentityServer();
            app.UseAuthorization();
            //for QuickStart-UI 启用静态文件
            app.UseStaticFiles();
          
            //app.UseMvc();
            app.UseMvcWithDefaultRoute(); //这里带有默认的路由
        }
    }
}
