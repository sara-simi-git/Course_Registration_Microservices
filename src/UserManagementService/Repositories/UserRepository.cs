using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Interfaces;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManagementContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(UserManagementContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersList()
        {
            _logger.LogInformation("DB: Retrieving all users");
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB error: Failed to retrieve users list");
                throw;
            }
        }

        public async Task<User?> GetUserById(string id)
        {
            _logger.LogInformation("DB: Fetching user - UserId: {UserId}", id);
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB error: Failed to fetch user - UserId: {UserId}", id);
                throw;
            }
        }

        public async Task<User> CreateUser(User user)
        {
            _logger.LogInformation("DB: Creating user - UserId: {UserId}", user.Id);
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("DB: User created - UserId: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB error: Failed to create user - UserId: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<User?> UpdateUser(User user)
        {
            _logger.LogInformation("DB: Updating user - UserId: {UserId}", user.Id);
            try
            {
                var existing = await _context.Users.FindAsync(user.Id);
                if (existing == null)
                {
                    _logger.LogWarning("DB: User not found for update - UserId: {UserId}", user.Id);
                    return null;
                }
                _context.Entry(existing).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("DB: User updated - UserId: {UserId}", user.Id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB error: Failed to update user - UserId: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUser(string id)
        {
            _logger.LogInformation("DB: Deleting user - UserId: {UserId}", id);
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("DB: User not found for delete - UserId: {UserId}", id);
                    return false;
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("DB: User deleted - UserId: {UserId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB error: Failed to delete user - UserId: {UserId}", id);
                throw;
            }
        }
    }
}
