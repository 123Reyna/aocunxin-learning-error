using System;
using System.Collections.Generic;
using System.Text;

namespace Aocunxin.Learning.RbtMQ
{
    public  interface ICallbackFunction
    {
        void ProcessMsgAsync(RbtMessage msg);
    }
}
