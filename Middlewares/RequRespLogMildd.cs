﻿using Aocunxin.Blog.Common.Helper;
using Aocunxin.Blog.Common.LogHelper;
using Aocunxin.Blog.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aocunxin.Blog.Middlewares
{
    public class RequRespLogMildd
    {
        private readonly RequestDelegate _next;

        public RequRespLogMildd(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (Appsettings.app("Middleware", "RequestResponseLog", "Enabled").ObjToBool())
            {
                // 过滤,只有接口
                if(context.Request.Path.Value.Contains("api"))
                {
                    // 确保可以多次读取数据,保存进了内存数据
                    context.Request.EnableBuffering();
                    Stream originalBody = context.Response.Body;

                    try
                    {
                        // 存储请求数据
                        await RequestDataLog(context);
                        using(var ms=new MemoryStream())
                        {
                            context.Response.Body = ms;
                            await _next(context);

                            // 存储响应数据
                            ResponseDataLog(context.Response, ms);
                            ms.Position = 0;
                            await ms.CopyToAsync(originalBody);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    finally
                    {
                        context.Response.Body = originalBody;
                    }
                } else
                {
                    await _next(context);
                }
            } else
            {
                await _next(context);
            }
        }

        private async Task RequestDataLog(HttpContext context)
        {
            var request = context.Request;
            var sr = new StreamReader(request.Body);
            var content = $"QueryData:{request.Path+request.QueryString}\r\n BodyData:{await sr.ReadToEndAsync()}";
            if(!string.IsNullOrEmpty(content))
            {
                Parallel.For(0, 1, e =>
                {
                    LogLock.OutSql2Log("RequestResponseLog", new string[] { "Request Data:", content });
                });
                //这就意味着只能读取一次
                request.Body.Position = 0;
            }
        }

        private void ResponseDataLog(HttpResponse response,MemoryStream ms)
        {
            ms.Position = 0;
            var ResponseBody = new StreamReader(ms).ReadToEnd();
            // 去除 Html
            var reg = "<[^>]+>";
            var isHtml = Regex.IsMatch(ResponseBody, reg);
            if(!string.IsNullOrEmpty(ResponseBody))
            {
                Parallel.For(0, 1, e =>
                {
                    LogLock.OutSql2Log("RequestResponseLog", new string[] { "Response Data:", ResponseBody });
                });
            };
        }
    }
}