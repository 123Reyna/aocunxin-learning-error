using Aocunxin.Learning.Common.Attributes;
using Aocunxin.Learning.Entity;
using Aocunxin.Learning.IRepository;
using Aocunxin.Learning.IRepository.UnitOfWork;
using Aocunxin.Learning.RbtMQ;
using Aocunxin.Learning.Repository;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Learning.Service
{
    [Service]
    public class AccountUserService : BaseRepository<AccountUser>, ICallbackFunction
    {

      
        public readonly IUnitOfWork getDb;
        public AccountUserService(IUnitOfWork db) : base(db)
        {
            getDb = db;
        }

        public void ProcessMsgAsync(RbtMessage msg)
        {

        }
    }
}
