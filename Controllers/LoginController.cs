using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Druzhbank.Controllers
{
    [ApiController]
    [Route("api")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [HttpGet("login")]
        public ActionResult<string> Login()
        {
            return "Здесь будет осуществляться логин в банк";
        }
    }
}