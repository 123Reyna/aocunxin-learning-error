using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aocunxin.Learning.Common
{
   public class MstResult
    {

        public class MstResultModel
        {
            public string Code { get; set; }

            public string Message { get; set; }

            public object Data { get; set; }

            public string MsgType { get; set; }
        }

        public static MstResultModel Success(string msg = null,object rtnData = null)
        {

             var Json =(new MstResultModel
            {
                Code = "200",
                Message = msg,
                Data = rtnData ?? string.Empty,
                MsgType = "Info"
            });
            return Json;

        }

        public static MstResultModel Error(string responseMessage,  object rtnData = null)
        {
            var Json = (new MstResultModel
            {
                Code = "400",
                Message = responseMessage ?? string.Empty,
                Data = rtnData ?? string.Empty,
                MsgType = "Error"
            });
            return Json;
        }
    }
}
