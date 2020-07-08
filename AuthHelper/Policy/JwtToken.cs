using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Aocunxin.Blog.AuthHelper.Policy
{
    public class JwtToken
    {

        public static dynamic BuildJwtToken(Claim[] claims,PermissionRequirement permissionRequirement)
        {
            var now = DateTime.Now;
            var jwt = new JwtSecurityToken(
                issuer:permissionRequirement.Issuer,
                audience:permissionRequirement.Audience,
                claims:claims,
                notBefore:now ,
                expires:now.Add(permissionRequirement.Expiration),
                signingCredentials:permissionRequirement.SigningCredentials
             );

            var encodeJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            // 打包返回前台
            var responseJson = new
            {
                success = true,
                token = encodeJwt,
                expires_in = permissionRequirement.Expiration.TotalSeconds,
                token_type = "Bearer"
            };
            return responseJson;
        }
    }
}
