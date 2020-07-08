using Aocunxin.Learning.Common.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Aocunxin.Learning.Common.Extensions
{
    /// <summary>
    /// 服务扫描注册器扩展
    /// </summary>
   public static class IocServiceScanner
    {
        public static IServiceCollection AddIocServiceScanner(this IServiceCollection service)
        {
            var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.DefinedTypes.Where(t => t.GetCustomAttribute<ServiceAttribute>(false) != null))
                .ToArray();
            foreach(var type in serviceTypes)
            {
                var serviceAttribute = type.GetCustomAttribute<ServiceAttribute>();
                var scopeType = serviceAttribute?.ScopeType ?? ScopeEnum.Scope;
                var interfaceType = serviceAttribute?.InterfaceType ?? type.GetInterface($"I{type.Name}");
                _ = interfaceType == null ? AddInstance(scopeType, service, type)
                   : AddByInterface(scopeType, service, interfaceType, type);
               
            }
            return service;
        }

        public static IServiceCollection AddInstance(
            ScopeEnum scopeType, IServiceCollection services, Type implementType)
            => scopeType switch
            {
                ScopeEnum.Scope => services.AddScoped(implementType),
                ScopeEnum.Singleton => services.AddSingleton(implementType),
                ScopeEnum.Transient => services.AddTransient(implementType),
                _ => services.AddScoped(implementType),
            };

        public static IServiceCollection AddByInterface(ScopeEnum scopeType, IServiceCollection services, Type interfaceType,
            Type implementType) =>
            scopeType switch
            {
                ScopeEnum.Scope => services.AddScoped(interfaceType, implementType),
                ScopeEnum.Singleton => services.AddSingleton(interfaceType, implementType),
                ScopeEnum.Transient => services.AddTransient(interfaceType, implementType),
                _ => services.AddScoped(interfaceType, implementType),
            };
    }
}
