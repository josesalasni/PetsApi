using Microsoft.AspNetCore.Mvc;

namespace petsapi.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public string get() 
        {
            return "App is Running";
        }
    }
}