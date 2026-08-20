using UserManagementService.DTOs;

namespace UserManagementService.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetUsersList();
        Task<UserResponseDto?> GetUserById(string id);
        Task<UserResponseDto?> CreateUser(UserCreateDto user);
        Task<UserResponseDto?> UpdateUser(string id, UserUpdateDto updateUser);
        Task<bool> DeleteUser(string id);
        Task<LoginResponseDto?> Authenticate(string id, string password);
    }
}
