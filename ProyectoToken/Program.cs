using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProyectoToken.Models;
using ProyectoToken.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add dbcontext
builder.Services.AddDbContext<DbPruebaContext>(option =>
{
    // call cadena sql for connection string from appsettings.json
    option.UseSqlServer(builder.Configuration.GetConnectionString("cadenaSQL"));
});

// agregar el autorizador
builder.Services.AddScoped<IAutorizacionService, AutorizacionService>();
// configurar nuestro jwt para dentro del proyecto (considera en colocar una cadena de 32 bytes minimo)
var Key = builder.Configuration.GetValue<string>("JwtSettings:key");
var keyBytes = Encoding.ASCII.GetBytes(Key);
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        // no valida desde donde solicita el usuario
        ValidateAudience = false,
        ValidateLifetime = true,
        // ningun tipo de desviacion en reloj del token
        ClockSkew = TimeSpan.Zero
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// agregar autenticacion
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
