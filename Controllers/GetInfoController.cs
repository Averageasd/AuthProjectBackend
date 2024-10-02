using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [Route("api/[controller]")]
    public class GetInfoController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public IActionResult Index()
        {
            return Ok(new
            {
                secretInfo = "secret info"
            });
        }
    }
}
