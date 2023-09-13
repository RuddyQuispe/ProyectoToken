using Microsoft.IdentityModel.Tokens;
using ProyectoToken.Models;
using ProyectoToken.Models.Custom;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProyectoToken.Services
{
    public class AutorizacionService : IAutorizacionService
    {
        private readonly DbPruebaContext _context;
        private readonly IConfiguration _configuracion;

        public AutorizacionService(DbPruebaContext context, IConfiguration configuracion)
        {
            _context = context;
            _configuracion = configuracion;
        }

        private string generarToken(string token)
        {
            // obtener key token de app settings json
            var Key = _configuracion.GetValue<string>("JwtSettings:key");
            var keyBytes = Encoding.ASCII.GetBytes(Key);
            // crear info del usuario
            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, token));
            // crear credencial para el token
            var credencialesToken = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature);
            // crear detalles del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = credencialesToken
            };
            // crear controladores del jwt
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            // obtener el token
            string tokenCreated = tokenHandler.WriteToken(tokenConfig);
            return tokenCreated;
        }

        public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest request)
        {
            var usuarioEncontrado = _context.Usuarios.FirstOrDefault(usr => usr.NombreUsuario == request.Nombreusuario && usr.Clave == request.Clave);
            if (usuarioEncontrado == null)
                return await Task.FromResult<AutorizacionResponse>(null);
            string tokenCreated = this.generarToken(usuarioEncontrado.IdUsuario.ToString());
            return new AutorizacionResponse()
            {
                Token = tokenCreated,
                Resultado = true,
                Mensaje = "Ok"
            };
        }
    }
}
