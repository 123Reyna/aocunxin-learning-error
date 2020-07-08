using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Aocunxin.Learning.Common.AppConfig;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;

namespace Aocunxin.Learning.Common.AuthHelper
{
   public class JwtHelper
    {
        /// <summary>
        /// 颁发JWT字符串
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string IssueJwt(TokenModelJwt tokenModel)
        {
            string iss = SysConfig.app(new string[] { "Audience", "Issuer" });
            string aud = SysConfig.app(new string[] { "Audience", "Audience" });
            string secret = AppSecretConfig.Audience_Secret_String;
            var claims = new List<Claim>
            {
                /*
                * 特别重要：
                  1、这里将用户的部分信息，比如 uid 存到了Claim 中，如果你想知道如何在其他地方将这个 uid从 Token 中取出来，请看下边的SerializeJwt() 方法，或者在整个解决方案，搜索这个方法，看哪里使用了！
                  2、你也可以研究下 HttpContext.User.Claims ，具体的你可以看看 Policys/PermissionHandler.cs 类中是如何使用的。
                */

                //“jti”（JWT ID）声明提供了JWT的唯一标识符。标识符值的分配方式必须确保同一个值被意外地分配给不同的数据对象；如果应用程序使用多个颁发者，必须防止值之间的冲突也由不同发行人制作。可以使用“jti”声明以防止JWT被重播。“jti”值就是一个例子-敏感字符串。使用此声明是可选的。
                new Claim(JwtRegisteredClaimNames.Jti,tokenModel.Uid.ToString()),
                //“iat”（签发时间）索赔确定了JWT发布。这项索赔可用于确定JWT的年龄。它的值必须是包含NumericDate值的数字。使用这个声明是可选的。
                new Claim(JwtRegisteredClaimNames.Iat,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),//发布时间 
                //确定了不接受JWT进行处理的时间
                  new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
                   //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
                new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(1000)).ToUnixTimeSeconds()}"),
                new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(1000).ToString()),
                new Claim(JwtRegisteredClaimNames.Iss,iss),//发行人
                new Claim(JwtRegisteredClaimNames.Aud,aud),//用户
            };
            // 可以将一个用户的多个角色全部赋予
            claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));
            // 密钥(SymmetricSecurityKey对安全性的要求,密钥的长度太短会报出异常
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(issuer: iss, claims: claims, signingCredentials: creds);
            var jwtHandler = new JwtSecurityTokenHandler();
            var encodedJwt = jwtHandler.WriteToken(jwt);
            return encodedJwt;
        }


        public static TokenModelJwt SerializeJwt(string jwtStr)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(jwtStr);
            object role;
            try
            {
                jwtToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            } catch(Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var tm = new TokenModelJwt
            {
                Uid = (jwtToken.Id).ObjToInt(),
                Role = role != null ? role.ObjToString() : "",
            };
            return tm;
        }
    }


    /// <summary>
    /// 令牌
    /// </summary>
    public class TokenModelJwt
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }

    }


}
