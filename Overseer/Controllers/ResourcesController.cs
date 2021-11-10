using Microsoft.AspNetCore.Mvc;

using OneClickDesktop.Api.Controllers;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ResourcesApiController
    {
        public override IActionResult GetResources()
        {
            throw new System.NotImplementedException();
        }
    }
}
