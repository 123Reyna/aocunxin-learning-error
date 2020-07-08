using System;
using System.Collections.Generic;
using System.Text;

namespace Aocunxin.Learning.RbtMQ
{
   public class RbtMessage
    {
        public string ID { get; set; }

        public string Title { get; set; }
        /// <summary>
        /// 消息的内容，发布的具体内容,如果是与socket通信的，这里的内容，必须是socketMessage的格式，否无法通信
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// queueName
        /// </summary>
        public string Routingkey { get; set; }



       

    }
}
