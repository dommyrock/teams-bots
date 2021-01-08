using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Teams_Bots.Controllers
{
    [Route("api/adaptive-card")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync(object payload)
        {
            try
            {
                var dataReceived = payload;
                return Ok(dataReceived);
            }
            catch (System.Exception)
            {
                return NotFound();
            }
        }
    }
}