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
               //�� OAuth ��ȣ�OIDC(OpenID Connect) �е� scopes �������� API�������������� �û�id���û��� �� �����ַ��������ݡ�
               .AddInMemoryIdentityResources(Config.IdentityResources);

            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                    .AddGoogle("Google", options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = "<insert here>";
                        options.ClientSecret = "<insert here>";
                    })
                    // ������֤
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

            //����IdentityServer
            app.UseIdentityServer();
            app.UseAuthorization();
            //for QuickStart-UI ���þ�̬�ļ�
            app.UseStaticFiles();
          
            //app.UseMvc();
            app.UseMvcWithDefaultRoute(); //�������Ĭ�ϵ�·��
        }
    }
}
