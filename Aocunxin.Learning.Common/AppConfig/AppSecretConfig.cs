using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Aocunxin.Learning.Common.AppConfig
{
   public class AppSecretConfig
    {
        private static string Audience_Secret = SysConfig.app(new string[] { "Audience", "Secret" });

        private static string Audience_Secret_File = SysConfig.app(new string[] { "Audience", "SecretFile" });

        public static string Audience_Secret_String => InitAudience_Secret();

        private static string InitAudience_Secret()
        {
            var securityString = DifDbConnOfSecurity(Audience_Secret_File);
            if(!string.IsNullOrEmpty(Audience_Secret_File)&&!string.IsNullOrEmpty(securityString))
            {
                return securityString;
            } else
            {
                return Audience_Secret;
            }
        }

        private static string DifDbConnOfSecurity(params string[] conn)
        {
            foreach(var item in conn)
            {
                try
                {
                    if(File.Exists(item))
                    {
                        return File.ReadAllText(item).Trim();
                    }
                }
                catch (System.Exception) { }
            }
            return conn[conn.Length - 1];
        }
    }
}
