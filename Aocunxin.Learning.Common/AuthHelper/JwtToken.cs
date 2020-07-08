using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Aocunxin.Learning.Common.AuthHelper
{
    /// <summary>
    /// JWTToken生成类
    /// </summary>
   public class JwtToken
    {

        public static dynamic BuildJwtToken(Claim[] claims,PermissionRequirement permissionRequirement)
        {
            var now = DateTime.Now;
            var jwt = new JwtSecurityToken(issuer: permissionRequirement.Issuer,
                audience: permissionRequirement.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(permissionRequirement.Expiration),
                signingCredentials: permissionRequirement.SigningCredentials
                );
            // 生成token
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            //打包返回前台
            var responseJson = new
            {
                success = true,
                token = encodedJwt,
                expires_in = permissionRequirement.Expiration.TotalSeconds,
                token_type = "Bearer"
            };
            return responseJson;
        }
    }
}
