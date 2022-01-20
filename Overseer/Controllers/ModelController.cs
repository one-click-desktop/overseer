using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using OneClickDesktop.Overseer.Services.Interfaces;

namespace OneClickDesktop.Overseer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : Controller
    {
        private readonly ISystemModelService modelService;
        private readonly IWebHostEnvironment env;
        
        public ModelController(ISystemModelService modelService, IWebHostEnvironment env)
        {
            this.modelService = modelService;
            this.env = env;
        }
        
        // GET
        public IActionResult Index()
        {
            if (!env.IsDevelopment())
                return NotFound();
            
            return Ok(modelService.GetSessionDump());
        }
    }
}