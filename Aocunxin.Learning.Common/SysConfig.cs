using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aocunxin.Learning.Common
{
    public class SysConfig
    {
        /// <summary>
        /// 系统配置json文件路径
        /// </summary>
        private static readonly string ConfigPath= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        /// <summary>
        /// 编译时不会报错,运行时才会报错,充当C#类型系统中的静态类型声明,
        /// var实际上编译器抛给我们的语法糖，一旦被编译，编译器就会自动匹配var变量的实际类型，并用实际类型来替换该变量的声明，等同于我们在编码时使用了实际类型声明。而dynamic被编译后是一个Object类型，编译器编译时不会对dynamic进行类型检查
        /// </summary>
        public static dynamic Params { get; set; }

        static IConfiguration Configuration { get; set; }
        public SysConfig()
        {

        }
        static string contentPath { get; set; }
        public SysConfig(string contentPath)
        {
            //如果你把配置文件 是 根据环境变量来分开了，可以这样写
            //Path = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";
            string Path = "appsettings.json";
            Configuration = new ConfigurationBuilder()
               .SetBasePath(contentPath)
               .Add(new JsonConfigurationSource { Path = Path, Optional = false, ReloadOnChange = true })//这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
               .Build();

        }


        /// <summary>
        /// 封装要操作的字符
        /// </summary>
        /// <param name="sections">节点配置</param>
        /// <returns></returns>
        public static string app(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";
        }
        public static void InitConfig()
        {
            try
            {
                if(!System.IO.File.Exists(ConfigPath))
                {
                    return;
                }
                try
                {
                    using(StreamReader file=File.OpenText(ConfigPath))
                    {
                        using(JsonTextReader reader =new JsonTextReader(file))
                        {
                            Params = JToken.ReadFrom(reader);
                        }
                    }
                } catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            } catch(Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
    }
}
