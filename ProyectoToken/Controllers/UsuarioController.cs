using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProyectoToken.Models.Custom;
using ProyectoToken.Services;

namespace ProyectoToken.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IAutorizacionService _autorizacionService;

        public UsuarioController(IAutorizacionService autorizacionService)
        {
            _autorizacionService = autorizacionService;
        }

        [HttpPost]
        [Route("Autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] AutorizacionRequest request)
        {
            var resultadoAutorizacion = await _autorizacionService.DevolverToken(request);
            if (resultadoAutorizacion == null)
                return Unauthorized();
            return Ok(resultadoAutorizacion);
        }
    }
}
