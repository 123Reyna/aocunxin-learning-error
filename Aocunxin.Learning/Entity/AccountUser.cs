using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Aocunxin.Learning.Entity
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("account_user")]
    public partial class AccountUser
    {
           public AccountUser(){


           }
             
           [SugarColumn(ColumnName= "Account")]
           public string Account { get;set;}

       
           [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
            public int Id {get;set;}


           [SugarColumn(ColumnName = "Password")]
           public string Password { get; set; }

           [SugarColumn(ColumnName = "UserName")]
            public string UserName { get; set; }

           [SugarColumn(ColumnName = "Token")]
           public string Token { get; set; }



    }
}
