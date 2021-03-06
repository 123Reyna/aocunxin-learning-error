﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aocunxin.Learning.AuthHelper;
using Aocunxin.Learning.Common.AuthHelper;
using Aocunxin.Learning.Common.Helper;
using Aocunxin.Learning.IService;
using Aocunxin.Learning.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using SqlSugar;

namespace Aocunxin.Learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {


        readonly SysUserInfoServices _sysUserInfoServices;
        readonly PermissionRequirement _requirement;
        private readonly IRoleModulePermissionServices _roleModulePermissionServices;


        /// <summary>
        /// 构造函数注入
        /// </summary>
        /// <param name="sysUserInfoServices"></param>

        public LoginController(SysUserInfoServices sysUserInfoServices, PermissionRequirement requirement, IRoleModulePermissionServices roleModulePermissionServices)
        {
            this._sysUserInfoServices = sysUserInfoServices;
            _requirement = requirement;
            _roleModulePermissionServices = roleModulePermissionServices;

        }

        #region 获取token的第1种方法
        /// <summary>
        /// 获取JWT的方法1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Token")]
        public async Task<object> GetJwtStr(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            // 通过数据库去调取数据,分配权限
            var user = await _sysUserInfoServices.GetUserRoleNameStr(name, MD5Helper.MD5Encrypt32(pass));
            if (user != null)
            {
                TokenModelJwt tokenModel = new TokenModelJwt { Uid = 1, Role = user };
                jwtStr = JwtHelper.IssueJwt(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }
            return Ok(new
            {
                success = suc,
                token = jwtStr
            });
        }

        #endregion

        [HttpGet]
        [Route("GetTokenNuxt")]
        public object GetJwtStrForNuxt(string name, string pass)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            //这里就是用户登陆以后，通过数据库去调取数据，分配权限的操作
            //这里直接写死了
            if (name == "admins" && pass == "admins")
            {
                TokenModelJwt tokenModel = new TokenModelJwt
                {
                    Uid = 1,
                    Role = "Admin"
                };

                jwtStr = JwtHelper.IssueJwt(tokenModel);
                suc = true;
            }
            else
            {
                jwtStr = "login fail!!!";
            }
            var result = new
            {
                data = new { success = suc, token = jwtStr }
            };

            return Ok(new
            {
                success = suc,
                data = new { success = suc, token = jwtStr }
            });
        }

        [HttpGet]
        [Route("JWTToken3.0")]
        public async Task<object> GetJwtToken3(string name = "", string pass = "")
        {
            string jwtStr = string.Empty;
            if(string.IsNullOrEmpty(name)||string.IsNullOrEmpty(pass))
            {
                return new JsonResult(new
                {
                    Status = false,
                    message = "用户名或密码不能为空"
                });
            }
            pass = MD5Helper.MD5Encrypt32(pass);
            var user = await _sysUserInfoServices.Query(d => d.uLoginName == name && d.uLoginPWD == pass);
            if (user.Count > 0)
            {
                var userRoles = await _sysUserInfoServices.GetUserRoleNameStr(name, pass);
                // 如果是基于用户，添加用户，如果是基于角色则添加角色
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,name),
                    new Claim(JwtRegisteredClaimNames.Jti,user.FirstOrDefault().uID.ToString()),
                    new Claim(ClaimTypes.Expiration,DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())
                };
                claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                var data = await _roleModulePermissionServices.RoleModuleMaps();
                var list = (from item in data
                            where item.IsDeleted == false
                            orderby item.Id
                            select new PermissionItem
                            {
                                Url = item.Module?.LinkUrl,
                                Role = item.Role?.Name,
                            }
                          ).ToList();
                _requirement.Permissions = list;

                // 用户标识
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);
                var token = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                return new JsonResult(token);

            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "认证失败"
                });
            }
        }


        [HttpGet]
        [Route("RefreshToken")]
        public async Task<object> RefreshToken(string token = "")
        {
            string jwtStr = string.Empty;
            if(string.IsNullOrEmpty(token))
            {
                return new JsonResult(new
                {
                    Status = false,
                    message = "token无效，请重新登录！"
                });
            }
            var tokenModel = JwtHelper.SerializeJwt(token);
            if (tokenModel != null && tokenModel.Uid > 0)
            {
                var user = await _sysUserInfoServices.QueryById(tokenModel.Uid);
                if (user != null)
                {
                    var userRoles = await _sysUserInfoServices.GetUserRoleNameStr(user.uLoginName, user.uLoginPWD);
                    var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.uLoginName),
                    new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ObjToString()),
                    new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                    claims.AddRange(userRoles.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

                    //用户标识
                    var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                    identity.AddClaims(claims);

                    var refreshToken = JwtToken.BuildJwtToken(claims.ToArray(), _requirement);
                    return new JsonResult(refreshToken);
                }
            }

            return new JsonResult(new
            {
                success = false,
                message = "认证失败"
            });
        }
     }
}

