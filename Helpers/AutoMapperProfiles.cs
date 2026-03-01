using AutoMapper;
using UserManagementAPI.Models;
using UserManagementAPI.DTOs;

namespace UserManagementAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Mapeamos la entidad User al DTO de respuesta para ocultar el Hash/Salt
            CreateMap<User, UserResponseDto>();
        }
    }
}