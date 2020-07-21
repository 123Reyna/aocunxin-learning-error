using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MVC_Client
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            //ͬ���������Ѿ��ر��� JWT Claim����ӳ�䣬�������õ�Claim������'sub'��'idp'��
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;

            });
            // AddAuthentication�������֤������ӵ� DI�� ����ʹ�� cookie �����ص�¼�û���ͨ����Cookies����ΪDefaultScheme�������ǽ� DefaultChallengeScheme ����Ϊ��oidc������Ϊ��������Ҫ�û���¼ʱ�����ǽ�ʹ��OpenID Connect Э�顣

            //Ȼ������ʹ�� AddCookie ��ӿ��Դ��� cookie �Ĵ������

//���AddOpenIdConnect��������ִ�� OpenID Connect Э��Ĵ������Authority�����������ε� IdentityServer ��ַ��Ȼ������ͨ��ClientId��ʶ������ͻ��ˡ� SaveTokens������ cookie �б�������IdentityServer �����ƣ��Ժ���Ҫ���ǣ�
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "http://localhost:49363/";
                options.RequireHttpsMetadata = false;
                options.ClientId = "mvc";
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.Scope.Add("api1");
                options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                options.Scope.Add(OidcConstants.StandardScopes.Profile);
                options.Scope.Add(OidcConstants.StandardScopes.OfflineAccess);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
          
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute()
                    .RequireAuthorization();
            });

            //�������Ĭ�ϵ�·��
        }
    }
}
