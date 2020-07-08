using Aocunxin.Learning.Common;
using Aocunxin.Learning.Entity;
using Aocunxin.Learning.IRepository.UnitOfWork;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Learning.AOther
{
    public class DBSeed
    {


        private readonly IUnitOfWork _unitOfWork;

        public DBSeed(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        private static string SeedDataFolder = "BlogCore.Data.json/{0}.tsv";


        // 这里我把重要的权限数据提出来的精简版，默认一个Admin_Role + 一个管理员用户，
        // 然后就是菜单+接口+权限分配，注意没有其他博客信息了，下边seeddata 的时候，删掉即可。

        // gitee 源数据
        private static string SeedDataFolderMini = "BlogCore.Mini.Data.json/{0}.tsv";
        /// <summary>
        /// 异步添加种子数据
        /// </summary>
        /// <param name="myContext"></param>
        /// <param name="WebRootPath"></param>
        /// <returns></returns>
        //public static  async Task SeedAsync(string WebRootPath)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(WebRootPath))
        //        {
        //            throw new Exception("获取wwwroot路径时，异常！");
        //        }

              



        //        // 创建数据库
        //        _unitOfWork.GetDbClient().DbMaintenance.CreateDatabase();

        //        Console.WriteLine("Create Tables...");
        //        // 创建表
        //        _unitOfWork.GetDbClient().CodeFirst.InitTables(
        //            typeof(Module),
        //            typeof(Permission),
        //            typeof(Role),
        //            typeof(RoleModulePermission),
        //            typeof(UserRole));

        //        // 后期单独处理某些表
        //        // myContext.Db.CodeFirst.InitTables(typeof(sysUserInfo));

        //        Console.WriteLine("Database is  created success!");
        //        Console.WriteLine();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}
