using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Aocunxin.Learning.Common.Attributes;
using Aocunxin.Learning.Entity;
using Aocunxin.Learning.IRepository.UnitOfWork;
using Aocunxin.Learning.RbtMQ;
using Aocunxin.Learning.Repository;
using Aocunxin.Learning.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SqlSugar;

namespace Aocunxin.Learning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public AccountUserService ms;

        public IUnitOfWork unitDal;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, AccountUserService _ms,
            IUnitOfWork _unitDal)
        {
            _logger = logger;
            ms = _ms;
            unitDal = _unitDal;
        }



        [HttpPost]
        [Route("SeedAsync")]
        public  async Task SeedAsync()
        {
            try
            {
                //if (string.IsNullOrEmpty(WebRootPath))
                //{
                //    throw new Exception("获取wwwroot路径时，异常！");
                //}

                // 创建数据库
                //  unitDal.GetDbClient().DbMaintenance.CreateDatabase();

              //var list=  unitDal.GetDbClient().SqlQueryable<Role>("select * from Role").ToList();

             
               // var list= _db.SqlQueryable<Role>("select * from Role").ToList();

                Console.WriteLine("Create Tables...");
                // 创建表
                unitDal.GetDbClient().CodeFirst.InitTables(
                    typeof(Module),
                    typeof(Permission),
                    typeof(Role),
                    typeof(RoleModulePermission),
                    typeof(UserRole));

                // 后期单独处理某些表
                // myContext.Db.CodeFirst.InitTables(typeof(sysUserInfo));

                Console.WriteLine("Database is  created success!");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("get")]
        //[UseTran]
        public int Get()
        {


            RbtMQSubscribe sub = new RbtMQSubscribe();
            ICallbackFunction prmsg = new AccountUserService(unitDal);
            RbtMessage msg = new RbtMessage();
            msg.Routingkey = "aocun.xin";
            //先订阅,需要先进行绑定接收，有需求先
            sub.BindReceiveMqMsg(null,prmsg, msg.Routingkey);


            return 0;
        }

        [HttpGet]
        [Route("aocunxin")]
        public int GetRoute()
        {
            RbtMessage msg = new RbtMessage();
            msg.Routingkey = "aocun.xin";
            msg.Title = "In";  //进仓 In,出仓 Out
                               //msg.From = "keith";
                               //msg.To = "weston";
            msg.Body = $"fff";
            RbtMQPublish.PushMsgToMq(msg);
            return 0;
        }


    }
}
