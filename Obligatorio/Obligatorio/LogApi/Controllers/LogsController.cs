using Microsoft.AspNetCore.Mvc;
using LogApi.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LogApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController: Controller
    {
        private IService service;
        public LogsController(IService service)
        {
            this.service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string usuario, string juego, string fecha)
        {
            List<string> logs = await service.GetMessages(usuario, juego,fecha);

            return Ok(logs);
        }
    }
}
