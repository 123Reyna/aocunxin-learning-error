using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.CompilerServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using Polly;
using Polly.Retry;
using System.Net.Sockets;

namespace Aocunxin.Learning.RbtMQ
{
    public class RbtMQConnection
    {
        public readonly static IConnectionFactory _connectionFactory;
        IConnection _connection;
        bool _disposed = false;

        object lock_sync = new object();
        static RbtMQConnection()
        {
            // IConfiguration 是用来加载配置值的，可以加载内存键值对、JSON或XML配置文件
            IConfiguration _configuration = new ConfigurationBuilder()
                                       .SetBasePath(AppDomain.CurrentDomain.BaseDirectory.ToString())
                                       .AddJsonFile("appsettings.json")
                                       .Build(); ;

            if (_configuration == null) throw new Exception("全局appSetting配置为空！无法连接_RabbitMQ!");


            var HostName = _configuration["RabbitMQ:HostName"];
            if (string.IsNullOrEmpty(HostName)) throw new Exception("RabbitMQ HostName 为必填项!");

            var UserName = _configuration["RabbitMQ:UserName"];
            if (string.IsNullOrEmpty(UserName)) throw new Exception("RabbitMQ UserName 为必填项!");

            var Password = _configuration["RabbitMQ:Password"];
            if (string.IsNullOrEmpty(Password)) throw new Exception("RabbitMQ Password 为必填项!");

            var VirtualHost = _configuration["RabbitMQ:VirtualHost"];
            if (string.IsNullOrEmpty(VirtualHost)) throw new Exception("RabbitMQ VirtualHost 为必填项!");

            int Port = Convert.ToInt32(_configuration["RabbitMQ:Port"]);
            if (Port == 0) throw new Exception("RabbitMQ Port 为必填项!");
            _connectionFactory = new ConnectionFactory()
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password,
                VirtualHost = VirtualHost,
                Port = Port,
                // 自动连接
                AutomaticRecoveryEnabled = true
            };
        }



        public bool IsConnected => this._connection != null && this._connection.IsOpen && this._disposed;
        public bool ConnectRbtMQ()
        {
            lock (this.lock_sync)
            {
                // 如果我们想指定处理多个异常类型通过OR即可
                //ConnectionFactory.CreateConnection期间无法打开连接时抛出异常
                RetryPolicy policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {

                    });// 重试次数，提供等待特定重试尝试的持续时间的函数，每次重试时调用的操作。
                policy.Execute(() =>
                 {
                     _connection = _connectionFactory.CreateConnection();

                 });

                if (IsConnected)
                {
                    //当连接被破坏时引发。如果在添加事件处理程序时连接已经被销毁，事件处理程序将立即被触发。
                   // _connection.ConnectionShutdown += this.OnConnectionShutdown;
                    //在连接调用的回调中发生异常时发出信号。当ConnectionShutdown处理程序抛出异常时，此事件将发出信号。如果将来有更多的事件出现在RabbitMQ.Client.IConnection上，那么这个事件当这些事件处理程序中的一个抛出异常时，它们将被标记。
                    //_connection.CallbackException += this.OnCallbackException;
                    //_connection.ConnectionBlocked += this.OnConnectionBlocked;
                    
                    return true;
                }
                else
                {
                    //MstCore.Pub.MstPub.Logs("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }

        }

        public IModel CreateModel()
        {
            if (!this.IsConnected)
            {
                this.ConnectRbtMQ();
            }
            return _connection.CreateModel();
        }


        //private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        // {
        //     if (this._disposed) return;
        //     //RabbitMQ连接正在关闭。 尝试重新连接...

        //     this.ConnectRbtMQ();
        // }

        // 关闭连接
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch
            {
            }
        }

    }
}
