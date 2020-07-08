using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using Newtonsoft.Json;
using System.Text;


namespace Aocunxin.Learning.RbtMQ
{
    public class RbtMQSubscribe
    {
        IConnection connection = null;
        IModel channel = null;


     
        //  Func<RbtMessage, bool> func,
        public void BindReceiveMqMsg(Func<RbtMessage, bool> func, ICallbackFunction function, string routingKey, string EXCHANGE_NAME = null, string exchange_type = null)
        {
            try
            {

                

                //创建与指定端点的连接。
                connection = RbtMQConnection._connectionFactory.CreateConnection();
                //创建并返回新的频道，会话和模型。
                channel = connection.CreateModel();

                bool IsFanout = false;//广播方式
                if (exchange_type == ExchangeType.Fanout)
                {
                    //广播方式
                    IsFanout = true;
                    EXCHANGE_NAME = EXCHANGE_NAME ?? "amq.topic";
                }
                else
                {
                    EXCHANGE_NAME = EXCHANGE_NAME ?? "amq.topic";
                    exchange_type = ExchangeType.Topic;
                }

                channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: exchange_type, durable: true);

                if (!IsFanout)
                {
                    this.channel.QueueDeclare(queue: routingKey,//队列名称
                                     durable: true,//是否持久化, 队列的声明默认是存放到内存中的，如果rabbitmq重启会丢失，如果想重启之后还存在就要使队列持久化，保存到Erlang自带的Mnesia数据库中，当rabbitmq重启之后会读取该数据库
                                     exclusive: false,//是否排外的，有两个作用，一：当连接关闭时connection.close()该队列是否会自动删除；二：该队列是否是私有的private，如果不是排外的，可以使用两个消费者都访问同一个队列，没有任何问题，如果是排外的，会对当前队列加锁，其他通道channel是不能访问的，如果强制访问会报异常：com.rabbitmq.client.ShutdownSignalException: channel error; protocol method: #method<channel.close>(reply-code=405, reply-text=RESOURCE_LOCKED - cannot obtain exclusive access to locked queue 'queue_name' in vhost '/', class-id=50, method-id=20)一般等于true的话用于一个队列只能有一个消费者来消费的场景
                                     autoDelete: false,//是否自动删除，当最后一个消费者断开连接之后队列是否自动被删除，可以通过RabbitMQ Management，查看某个队列的消费者数量，当consumers = 0时队列就会自动删除
                                     arguments: null);//队列中的消息什么时候会自动被删除？
                }
                else
                {
                    routingKey = channel.QueueDeclare().QueueName;
                        
                }
                channel.QueueBind(routingKey, EXCHANGE_NAME, "amq.topic");

                //Map<String, Object> args = new HashMap<String, Object>();
                //args.put("x-max-length", 10);
                // channel.QueueDeclare("routingKey", false, false, false, args);

                //（Spec方法）配置Basic内容类的QoS参数。
                //第一个参数是可接收消息的大小的  0不受限制
                //第二个参数是处理消息最大的数量  1 那如果接收一个消息，但是没有应答，则客户端不会收到下一个消息，消息只会在队列中阻塞
                //第三个参数则设置了是不是针对整个Connection的，因为一个Connection可以有多个Channel，如果是false则说明只是针对于这个Channel的。
                // basic.qos是针对channel进行设置的，也就是说只有在channel建立之后才能发送basic.qos信令。

                //在rabbitmq的实现中，每个channel都对应会有一个rabbit_limiter进程，当收到basic.qos信令后，在rabbit_limiter进程中记录信令中prefetch_count的值，同时记录的还有该channel未ack的消息个数。
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);//该方法可以看出休息小半秒和一秒的区别

                //构造函数，它将Model属性设置为给定值。
                // EventingBasicConsumer是基于长连接，发布订阅模式的消费方式，节省资源且实时性好，这是开发中最常用的消费模式。在一些需要消费者主动获取消息的场合，我们可以使用Get方式，Get方式是基于短连接的，请求响应模式的消费方式。
                EventingBasicConsumer consumer = new EventingBasicConsumer(this.channel);
                // 2.种 
                //BasicGetResult result = channel.BasicGet(queue:"routingKey",autoAck:true);
                //接收到消息时触发的事件
                consumer.Received += (model, bdea) =>
                {
                    byte[] body = bdea.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    if (message == null || message.Length == 0)
                    {
                        // 将BasicConsume方法的autoAck设置为false,然后处理一条消息后手动确认一下，这样的话已处理的消息在接收到确认回执时被删除，未处理的消息以Unacked状态存放在queue中。如果消费者挂了，Unacked状态的消息会自动重新变成Ready状态，如此一来就不用担心消息丢失,true会一次性发完
                        this.channel.BasicAck(deliveryTag: bdea.DeliveryTag, multiple: false);

                        return;
                    }

                   // 开启持久化功能，需同时满足：消息投递模式选择持久化、交换器开启持久化、队列开启持久化

                    RbtMessage mqMsg = JsonConvert.DeserializeObject<RbtMessage>(message);

                    function.ProcessMsgAsync(mqMsg);
                    //通过回调来确认是否处理成功,如果成功，返回true,表示确认OK
                    bool result = true;
                    if (func != null)
                    {
                        result = func(mqMsg);
                    }
                    if (result)
                    {
                        //（Spec方法）确认一个或多个已传送的消息。
                        channel.BasicAck(deliveryTag: bdea.DeliveryTag, multiple: false);
                    }
                };
                channel.BasicConsume(queue: routingKey, autoAck: false, consumer: consumer);

            }
            catch (Exception ex)
            {
                // MstCore.Pub.MstPub.Logs("RabbitMQ出错:" + ex.ToString());
            }
        }


        public void Dispose()
        {
            if (channel != null)
            {
                channel.Close();
            }

            if (connection != null)
            {
                connection.Close();
            }
        }
    }
}
