using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bloqueo global: Debes estar logueado para cualquier acción
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UsersController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Obtener todos los usuarios (SOLO ADMIN)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();

            // Usamos AutoMapper para convertir la lista de User a UserResponseDto
            var usersToReturn = _mapper.Map<IEnumerable<UserResponseDto>>(users);

            return Ok(usersToReturn);
        }

        // Obtener un usuario por ID (Cualquier usuario logueado)
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            // Uso de FindAsync por ser búsqueda por Llave Primaria
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "El usuario no existe en la base de datos." });
            }

            return Ok(_mapper.Map<UserResponseDto>(user));
        }

        // Eliminar un usuario (SOLO ADMIN)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "No se pudo eliminar: Usuario no encontrado." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Usuario con ID {id} eliminado correctamente." });
        }
    }
}