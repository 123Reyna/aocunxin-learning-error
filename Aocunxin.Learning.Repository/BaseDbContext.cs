using Aocunxin.Learning.Common;
using Aocunxin.Learning.Common.LogHelper;
using Newtonsoft.Json;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aocunxin.Learning.Repository
{
   public class BaseDbContext
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public static List<string> ListConn { get; set; }


        private static ISqlSugarClient _db
        {
            get
            {
                string connStr = null;
                // 配置从库
                var slaveConnectionConfigs = new List<SlaveConnectionConfig>();
                for(var i = 0; i < ListConn.Count; i++)
                {
                    if (i == 0)
                    {
                        connStr = ListConn[0];// 主数据库的数据
                    } else
                    {
                        slaveConnectionConfigs.Add(new SlaveConnectionConfig()
                        {
                            //HitRate表示权重 值越大执行的次数越高，如果想停掉哪个连接可以把HitRate设为0
                            HitRate = i * 2,
                            ConnectionString=ListConn[i]
                        });

                    }
                    
                }

                // 如果配置了SlaveConnectionConfigs那就是主从模式,所有的写入删除更新都在主库,查询走从库
                var db = new SqlSugarClient(new ConnectionConfig()
                {
                    ConnectionString = connStr,
                    DbType = (DbType)(int)SysConfig.Params.DbType,
                    IsAutoCloseConnection = true,
                    SlaveConnectionConfigs = slaveConnectionConfigs,
                    //(默认SystemTable)初始化主键和自增列信息的方式(注意：如果是数据库权限受管理限制或者找不到主键一定要设成attribute)
                    // InitKeyType.Attribute 表示从实体类的属性中读取 主键和自增列的信息(适合有独立的运维组的用户没有系统表操作权限)
                    InitKeyType = InitKeyType.Attribute
                });
                db.Ado.CommandTimeOut = 30000;// 设置超时时间
                db.Aop.OnLogExecuted = (sql, pars) => //SQL执行完事件
                {
                    LogLock.OutSql2Log("SqlLog", new string[] { GetParas(pars), "【SQL语句】：" + sql });
                };
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    if (db.TempItems == null)
                        db.TempItems = new Dictionary<string, object>();
                };
                db.Aop.OnError = (exp) =>// 执行SQL错误事件
                {
                    LogLock.OutSql2Log("SqlLog", new string[] { "【SQL错误】：" + exp.Sql });
                    throw new Exception(exp.Message);
                };
                db.Aop.OnDiffLogEvent = (it) =>// 可以方便拿到数据库操作前和操作后的数据变化
                {
                    var editBeforeData = it.BeforeData;//变化前的数据
                    var editAfterData = it.AfterData;// 变化后的数据
                    var sql = it.Sql;//SQL
                    var parameter = it.Parameters;// 参数
                    var data = it.BusinessData;// 业务数据
                    var time = it.Time ?? new TimeSpan();
                    var diffType = it.DiffType;//枚举值 insert,update,和delete用来做业务区分
                                               //你可以在这里面写日志方法
                    var log = $"时间:{time.TotalMilliseconds}\r\n";
                    log += $"类型:{diffType.ToString()}\r\n";
                    log += $"SQL:{sql}\r\n";
                    log += $"参数:{GetParas(parameter)}\r\n";
                    log += $"业务数据:{JsonConvert.SerializeObject(data)}\r\n";
                    log += $"变化前数据:{JsonConvert.SerializeObject(editBeforeData)}\r\n";
                    log += $"变化后数据:{JsonConvert.SerializeObject(editAfterData)}\r\n";
                    LogLock.OutSql2Log("SqlLog", new string[] { "【数据前后】：" + log });

                };

                return db;
            }

           

        }


        public static ISqlSugarClient Db
        {
            get { return _db; }
        }
        private static string GetParas(SugarParameter[] pars)
        {
            string key = "【SQL参数】：";
            foreach (var param in pars)
            {
                key += $"{param.ParameterName}:{param.Value}\n";
            }

            return key;
        }

        public static void SetConn()
        {
            var connMain = SysConfig.Params.ConnMain;
            //var connFrom = SysConfig.Params.ConnFrom;
            ListConn =new List<string> { connMain.ToString() };
        }

        public static void SetConn(List<string> listKey)
        {
            ListConn = new List<string>();
            foreach(var t in listKey)
            {
                ListConn.Add((string)SysConfig.Params[t]);
            }
        }

        public static void SetConn(string serverIp,string user,string pass,string dataBase)
        {
            ListConn = new List<string>();
            switch ((DbType)(int)SysConfig.Params.DbType)
            {
                case DbType.SqlServer:
                    ListConn.Add($"server={serverIp};user id={user};password={pass};persistsecurityinfo=true;database={dataBase}");
                    break;
                case DbType.MySql:
                    ListConn.Add($"Server={serverIp};Database={dataBase};Uid={user};Pwd={pass};");
                    break;
                case DbType.Oracle:
                    ListConn.Add($"Server=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={serverIp})(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={dataBase})));User Id={user};Password={pass};Persist Security Info=True;Enlist=true;Max Pool Size=300;Min Pool Size=0;Connection Lifetime=300");
                    break;
                case DbType.PostgreSQL:
                    ListConn.Add($"PORT=5432;DATABASE={dataBase};HOST={serverIp};PASSWORD={pass};USER ID={user}");
                    break;
                case DbType.Sqlite:
                    ListConn.Add($"Data Source={serverIp};Version=3;Password={pass};");
                    break;
            }
        }
    }
}
