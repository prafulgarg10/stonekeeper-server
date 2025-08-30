using System;
using Microsoft.EntityFrameworkCore;
using Stonekeeper.Data;
using Stonekeeper.Models;

namespace Stonekeeper.Service;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthService(ApplicationDbContext dbContext){
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Appuser>> GetAllUsersAsync()
    {
        return await _dbContext.Appusers.ToListAsync();
    }

    public async Task<Appuser?> FindByNameAsync(string userName)
    {
        var user = await _dbContext.Appusers.Where(u => u.Username.ToLower()==userName.ToLower()).FirstOrDefaultAsync();
        if(user!=null){
            var role = await _dbContext.Roles.Where(r => r.Id==user.RoleId).FirstOrDefaultAsync();
            user.Role = role!=null ? role : user.Role;
        }
        return user;
    }

    public bool CheckPassword(Appuser user, string password)
    {
        string passwordHash = user.Password;
        if(!string.IsNullOrEmpty(passwordHash)){
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        return false;
    }

    public async Task<int?> CreateAsync(Appuser user)
    {
        try{
            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        catch(Exception e){
            Console.WriteLine("Error while adding user", e);
            return null;
        }
        return user.Id;
    }
}
