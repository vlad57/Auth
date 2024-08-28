using API_Custom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Custom.Controllers
{
    [Route("api/test")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("get-test")]
        public IActionResult GetTest()
        {
            var oui = "";

            return Ok();
        }
    }
}
