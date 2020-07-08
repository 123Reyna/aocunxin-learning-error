using Aocunxin.Blog.Common.Helper;
using Aocunxin.Blog.Common.LogHelper;
using Aocunxin.Blog.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Blog.Middlewares
{
    public class IPLogMildd
    {

        private readonly RequestDelegate _next;

        public IPLogMildd(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (Appsettings.app("Middleware", "IPLog", "Enabled").ObjToBool())
            {
                // 过滤,只有接口
                if(context.Request.Path.Value.Contains("api"))
                {
                    context.Request.EnableBuffering();
                    try
                    {
                        // 存储请求数据
                        var request = context.Request;
                        var requestInfo = JsonConvert.SerializeObject(new RequestInfo()
                        {
                            Ip=GetClientIP(context),
                            Url=request.Path.ObjToString().TrimEnd('/').ToLower(),
                            Datetime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Date=DateTime.Now.ToString("yyyy-mm-dd"),
                            Week=GetWeek()

                        });
                        if(!string.IsNullOrEmpty(requestInfo))
                        {
                            // 会创建多个线程并行循环
                            Parallel.For(0, 1, e =>
                            {
                                LogLock.OutSql2Log("RequestIpInfoLog", new string[]
                                {
                                    requestInfo+","
                                }, false);
                            });
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        private string GetWeek()
        {
            string week = string.Empty;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    week = "周一";
                    break;
                case DayOfWeek.Tuesday:
                    week = "周二";
                    break;
                case DayOfWeek.Wednesday:
                    week = "周三";
                    break;
                case DayOfWeek.Thursday:
                    week = "周四";
                    break;
                case DayOfWeek.Friday:
                    week = "周五";
                    break;
                case DayOfWeek.Saturday:
                    week = "周六";
                    break;
                case DayOfWeek.Sunday:
                    week = "周日";
                    break;
                default:
                    week = "N/A";
                    break;
            }
            return week;
        }

        public static string GetClientIP(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].ObjToString();
            if(string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ObjToString();
            }
            return ip;
        }
    }
}
