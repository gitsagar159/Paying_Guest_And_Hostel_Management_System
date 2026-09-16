using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PGM.Areas.Tenant.Controllers
{
    [ApiController]
    [Area("Tenant")]
    [Route("api/[area]/[controller]")]
    [Authorize(AuthenticationSchemes = "TenantJwt")]
    public class TenantController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Area = "Tenant", Message = "Hello from Tenant area" });
        }
    }
}
