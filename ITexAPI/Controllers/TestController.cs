using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITexAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "ITexAPI is running!", DateTime = DateTime.Now });
        }
    }
}
