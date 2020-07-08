using Aocunxin.Learning.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Learning.IService
{
    public interface IRoleModulePermissionServices
    {
        Task<List<RoleModulePermission>> WithChildrenModel();
        Task<List<TestMuchTableResult>> QueryMuchTable();
        Task<List<RoleModulePermission>> RoleModuleMaps();
    }
}
