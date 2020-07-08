using Aocunxin.Learning.Common.AuthHelper;
using Aocunxin.Learning.IService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aocunxin.Learning.AuthHelper
{
    /// <summary>
    /// 权限授权处理器
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        public IAuthenticationSchemeProvider Schemes { get; set; }

        private readonly IRoleModulePermissionServices _roleModulePermissionServices;
        private readonly IHttpContextAccessor _accessor;


        public PermissionHandler(IAuthenticationSchemeProvider schemes, IRoleModulePermissionServices roleModulePermissionServices, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            Schemes = schemes;
            _roleModulePermissionServices = roleModulePermissionServices;
        }

        /// <summary>
        /// 重写异步处理程序
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
        {
            var httpContext = _accessor.HttpContext;
            if(!requirement.Permissions.Any())
            {
                var data = await _roleModulePermissionServices.RoleModuleMaps();
                var list = (from item in data
                            where item.IsDeleted == false
                            orderby item.Id
                            select new PermissionItem
                            {
                                Url = item.Module?.LinkUrl,
                                Role=item.Role?.Id.ObjToString(),
                            }
                          ).ToList();
                requirement.Permissions = list;
            }

            if (httpContext != null)
            {
                var questUrl = httpContext.Request.Path.Value.ToLower();
                // 判断请求是否停止
                var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
                // 在AuthenticateAsync代码中，先查询Scheme，然后根据SchemeName查询Handle，再调用handle的同名方法。
                foreach (var scheme in await Schemes.GetRequestHandlerSchemesAsync())
                {
                    if(await handlers.GetHandlerAsync(httpContext,scheme.Name) is IAuthenticationRequestHandler handler && await handler.HandleRequestAsync())
                    {
                        context.Fail();
                        return;
                    }

                }
                // 判断请求是否拥有凭据,即有没有登录
                var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
                if (defaultAuthenticate != null)
                {
                    var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                    if (result?.Principal != null)
                    {
                        httpContext.User = result.Principal;
                        if (true)
                        {
                            // 获取当前用户的角色信息
                            var currentUserRoles = (from item in httpContext.User.Claims
                                                    where item.Type == requirement.ClaimType
                                                    select item.Value).ToList();
                            var isMatchRole = false;
                            var permisssionRoles = requirement.Permissions.Where(w => currentUserRoles.Contains(w.Role));
                            foreach(var item in permisssionRoles)
                            {
                                try
                                {
                                    if (Regex.Match(questUrl, item.Url?.ObjToString().ToLower())?.Value == questUrl)
                                    {
                                        isMatchRole = true;
                                        break;
                                    }
                                }
                                catch (Exception)
                                {

                                }
                            }

                            if (currentUserRoles.Count <= 0 || !isMatchRole)
                            {
                                context.Fail();
                                return;
                            }


                        }

                        //判断过期时间（这里仅仅是最坏验证原则，你可以不要这个if else的判断，因为我们使用的官方验证，Token过期后上边的result?.Principal 就为 null 了，进不到这里了，因此这里其实可以不用验证过期时间，只是做最后严谨判断）
                        if ((httpContext.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration)?.Value) !=null&&DateTime.Parse(httpContext.User.Claims.SingleOrDefault(s=>s.Type==ClaimTypes.Expiration)?.Value)>=DateTime.Now)
                        {
                            context.Succeed(requirement);
                        } else
                        {
                            context.Fail();
                            return;
                        }
                        return;
                    }
                }
                // 判断没有登录时,是否访问登录的url,并且是post请求，并且是form表单提交类型，否则为失败
                if(!questUrl.Equals(requirement.LoginPath.ToLower(),StringComparison.Ordinal)&&(!httpContext.Request.Method.Equals("POST")||!httpContext.Request.HasFormContentType))
                {
                    context.Fail();
                    return;
                }
            }
            context.Succeed(requirement);
        }
    }
}
