using System;
using MyFirstServer.Models;

namespace MyFirstServer;

public interface IAuthService
{
    Task<IEnumerable<AppUser>> GetAllUsersAsync();
    Task<AppUser?> FindByNameAsync(string userName);
    bool CheckPassword(AppUser user, string password);
    Task<int?> CreateAsync(AppUser user); 
}
