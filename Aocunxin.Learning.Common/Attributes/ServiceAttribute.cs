using System;
using System.Runtime.CompilerServices;

namespace Aocunxin.Learning.Common.Attributes
{
  public class ServiceAttribute : Attribute
    {

        public ScopeEnum ScopeType { get; set; }

        public Type InterfaceType { get; set; }

        public ServiceAttribute(Type interfaceType=null,ScopeEnum scope = ScopeEnum.Scope)
        {
            InterfaceType= interfaceType;
            ScopeType = scope;

        }
    }

    public enum ScopeEnum
    {
        Singleton,
        Scope,
        Transient
    }
}
