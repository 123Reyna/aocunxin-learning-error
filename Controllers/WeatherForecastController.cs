using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Aocunxin.Blog.IService;
using Microsoft.AspNetCore.Authorization;

namespace aocunxin.Blog.Controllers
{
    [ApiController]
    [Route("[controller]")]
  
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

       
        readonly IHsbillService _service;

       
        public WeatherForecastController( IHsbillService service)
        {
         
            this._service = service;
        }



        [HttpGet]
        public async Task<object> Get()
        {
            
            
           var model= await _service.QueryById(2);
            var rng = new Random();
            return Ok(model);
        }
    }
}
