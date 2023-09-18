using Microsoft.IdentityModel.Tokens;
using ProyectoToken.Models;
using ProyectoToken.Models.Custom;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        private string generarRefreshToken()
        {
            var byteArray = new byte[64];
            var refreshtoken = "";
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(byteArray);
                refreshtoken = Convert.ToBase64String(byteArray);
            }
            return refreshtoken;
        }

        private async Task<AutorizacionResponse> guardarHistorialRefreshToken(int idUsuario, string token, string refreshtoken)
        {
            var historialrefreshToken = new HistorialRefreshToken
            {
                IdUsuario = idUsuario,
                Token = token,
                RefreshToken = refreshtoken,
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(2)
            };
            await _context.HistorialRefreshTokens.AddAsync(historialrefreshToken);
            await _context.SaveChangesAsync();
            return new AutorizacionResponse
            {
                Token = token,
                RefreshToken = refreshtoken,
                Resultado = true,
                Mensaje = "Ok"
            };
        }

        public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest request)
        {
            var usuarioEncontrado = _context.Usuarios.FirstOrDefault(usr => usr.NombreUsuario == request.Nombreusuario && usr.Clave == request.Clave);
            if (usuarioEncontrado == null)
                return await Task.FromResult<AutorizacionResponse>(null);
            string tokenCreated = this.generarToken(usuarioEncontrado.IdUsuario.ToString());
            string refreshToken = this.generarRefreshToken();
            return await this.guardarHistorialRefreshToken(usuarioEncontrado.IdUsuario, tokenCreated, refreshToken);
        }

        public async Task<AutorizacionResponse> DevolverRefreshToken(RefreshTokenRequest requestTokenRequest, int idusuario)
        {
            var refreshtokenEncontrado = _context.HistorialRefreshTokens.FirstOrDefault(x => x.Token == requestTokenRequest.TokenExpirado && x.RefreshToken == requestTokenRequest.RefreshToken && 
                                                                                                x.IdUsuario == idusuario);
            if (refreshtokenEncontrado is null)
                return new AutorizacionResponse
                {
                    Resultado = false,
                    Mensaje = "no existe refresh token"
                };
            var refreshTokenCrado = generarRefreshToken();
            var tokenCreado = this.generarToken(idusuario.ToString());
            return await guardarHistorialRefreshToken(idusuario, tokenCreado, refreshTokenCrado);
        }
    }
}
