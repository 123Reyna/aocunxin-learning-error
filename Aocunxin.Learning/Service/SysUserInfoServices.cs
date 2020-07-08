using Aocunxin.Learning.Common.Attributes;
using Aocunxin.Learning.Entity;
using Aocunxin.Learning.IRepository.UnitOfWork;
using Aocunxin.Learning.Repository;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Learning.Service
{
    [Service]
    public class SysUserInfoServices: BaseRepository<SysUserInfo>
    {
        public SysUserInfoServices(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<string> GetUserRoleNameStr(string loginName,string loginPwd)
        {
            string roleName = "";
            var userList = (await Query(a => a.uLoginName == loginName && a.uLoginPWD == loginPwd)).FirstOrDefault();
            if (userList != null)
            {
                // 获取角色
                var roleList = await Db.Queryable<Role>().Where(a => a.IsDeleted == false).ToListAsync();
                // 获取用户角色
                var userRoles = await Db.Queryable<UserRole>().Where(a => a.UserId == userList.uID).ToListAsync();
                if (userRoles.Count > 0)
                {
                    var arr = userRoles.Select(ur => ur.RoleId.ObjToString()).ToList();
                    var roles = roleList.Where(d => arr.Contains(d.Id.ObjToString()));
                    roleName = string.Join(',', roles.Select(r => r.Name).ToArray());
                }


            }
            return roleName;
        }
    }
}
