namespace UserManagementAPI.Services
{
    public interface IAuthService
    {
        string CreateToken(UserManagementAPI.Models.User user);
    }
}