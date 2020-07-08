using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aocunxin.Learning.RbtMQ
{
   public class RbtMQPublish
    {
        static RbtMQConnection connection = null;

        static RbtMQPublish()
        {
            connection = new RbtMQConnection();
        }
        /// <summary>
        /// 添加信息到队列
        /// </summary>
        public static void PushMsgToMq(RbtMessage msg, string EXCHANGE_NAME = null, 
            string EXCHANGE_TYPE = ExchangeType.Topic)
        {

            bool IsFanout = false;
            string routingKey = msg.Routingkey;

          

            if (EXCHANGE_TYPE == ExchangeType.Fanout)
            {
                IsFanout = true; //广播模式
              //  routingKey = "";
                EXCHANGE_NAME = EXCHANGE_NAME ?? "amq.topic";
            }
            else
            {
                EXCHANGE_NAME = EXCHANGE_NAME ?? "amq.topic";
            }


            try
            {
                if (connection == null)
                {
                    return;
                }

                string jsonMsg = JsonConvert.SerializeObject(msg);
                using (IModel channel = connection.CreateModel())
                {
                    if (IsFanout)
                    {
                        channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Fanout);
                    }
                    else
                    {

                        channel.QueueDeclare(queue: routingKey,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    }

                   IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    byte[] body = Encoding.UTF8.GetBytes(jsonMsg);
                    channel.BasicPublish(exchange: EXCHANGE_NAME,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
                }
            }
            catch (Exception ex)
            {
                // MstCore.Pub.MstPub.Logs("RabbitMQ出错:" + ex.ToString());
            }
        }


       

    }
}
