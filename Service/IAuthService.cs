using System;
using MyFirstServer.Models;

namespace MyFirstServer;

public interface IAuthService
{
    Task<IEnumerable<Appuser>> GetAllUsersAsync();
    Task<Appuser?> FindByNameAsync(string userName);
    bool CheckPassword(Appuser user, string password);
    Task<int?> CreateAsync(Appuser user); 
}
