using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aocunxin.Learning.Common;
using Aocunxin.Learning.Entity;
using Aocunxin.Learning.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Aocunxin.Learning.Common.MstResult;

namespace Aocunxin.Learning.Controllers
{
    //Produces 作用是指定response的content-type为application/json
    [Produces("application/json")]
    [Route("api/[controller]")]
    // 使用Controller渲染普通视图。ApiController操作仅返回已序列化并发送到客户端的数据
    [ApiController]
    public class AccountUserController : ControllerBase
    {

        public AccountUserService ms;
        public AccountUserController( AccountUserService _ms)
        {
          
            ms = _ms;
        }


        [HttpGet]
        [Route("GetAllAccount")]
        public async Task<MstResultModel> GetAllAccount()
        {
            var data = await ms.Query(s=>new { s.Account});
            return Success(null, data);

        }

        [HttpGet]
        public async Task<MstResultModel> Get(string account)
        {
            var data= await ms.Query(s=>s.Account==account);
            return Success(null,data) ;
            
        }

        [HttpPost]
        public async Task<MstResultModel> Update(AccountUser model)
        {
            if( ms.IsExist(s=>s.Account==model.Account))
            {
                 bool bl=  await ms.Update(model);
                return bl ? Success("修改成功") : Error("修改失败");
            } else
            {
               int id= await ms.Add(model);
                return id > 0 ? Success("新增成功") : Error("新增失败");
            }
        }






    }
}