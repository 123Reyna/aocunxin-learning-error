using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aocunxin.Blog.Filter
{
    public class UseServiceDIAttribute: ActionFilterAttribute
    {

        protected readonly ILogger<UseServiceDIAttribute> _logger;

        private readonly string _name;

    }
}
