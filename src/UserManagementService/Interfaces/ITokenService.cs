using UserManagementService.Models;

namespace UserManagementService.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string email, string fullName, UserRole role);
    }
}
