﻿using ProyectoToken.Models.Custom;

namespace ProyectoToken.Services
{
    public interface IAutorizacionService
    {
        Task<AutorizacionResponse> DevolverToken(AutorizacionRequest request);
        Task<AutorizacionResponse> DevolverRefreshToken(RefreshTokenRequest requestTokenRequest, int idusuario);
    }
}
