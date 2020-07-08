using Aocunxin.Learning.AuthHelper;
using Aocunxin.Learning.Common;
using Aocunxin.Learning.Common.AppConfig;
using Aocunxin.Learning.Common.AuthHelper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Aocunxin.Learning.AuthHelper
{

    /// <summary>
    /// Db 启动服务
    /// </summary>
    public static class AuthorizationSetup
    {

        public static void AddAuthorizationSetup(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Client", policy => policy.RequireRole("Client").Build());
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin").Build());
                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("Admin", "System"));

            });
            // 读取配置文件
            var symmetricKeyAsBase64 = AppSecretConfig.Audience_Secret_String;
            var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64);
            var signingKey= new  SymmetricSecurityKey(keyByteArray);
            var issuer = SysConfig.app(new string[] { "Audience", "Issuer" });
            var Audience = SysConfig.app(new string[] { "Audience", "Audience" });
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // 如果要数据库动态绑定,这边处理
            var permission = new List<PermissionItem>();

            var permissionRequirement = new PermissionRequirement("/api/denied",// 拒绝授权的跳转地址(目前无用)
               permission,
               ClaimTypes.Role,// 基于角色的授权
               issuer,// 发行人
               Audience,// 听众
               signingCredentials,// 签名凭据
               expiration: TimeSpan.FromSeconds(60 * 60)// 接口的过期时间
               );

            // 复杂的策略授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.Name,
                    policy => policy.Requirements.Add(permissionRequirement));
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = issuer,// 发行人
                ValidateAudience = true,
                ValidAudience = Audience,//订阅人
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                RequireExpirationTime = true
            };

            // 自带官方JWT认证
            // 开启Bearer认证
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = nameof(ApiResponseHandler);
                o.DefaultForbidScheme = nameof(ApiResponseHandler);
            })
            // 添加JwtBearer服务
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = tokenValidationParameters;
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // 如果过期,则把<是否过期>添加到,返回头信息中
                        if (context.Exception.GetType() == typeof(SecurityTokenException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiResponseHandler>(nameof(ApiResponseHandler), o => { });

            // 注入权限处理器
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddSingleton(permissionRequirement);
        }
    }
}
