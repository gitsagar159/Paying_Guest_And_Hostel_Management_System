using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PGM.Areas.Admin.Controllers
{

    [ApiController]
    [Area("Admin")]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "AdminJwt", Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet]
        [Route("GetStatus")]
        public IActionResult GetStatus()
        {
            return Ok(new { Area = "Admin", Message = "Hello from Admin area" });
        }
    }
}
