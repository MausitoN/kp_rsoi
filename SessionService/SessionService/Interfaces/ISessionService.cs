using SessionService.Models;
using SessionService.UserDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionService.Interfaces
{
    public interface ISessionService
    {
        Task<ServiceResponseSession> GetUsers();
        Task<ServiceResponseSession> AddUser(NewUser request);
        Task<Models.User> GetIdentity(string username, string password);
        Task<UserDto> CreateUserAsync(NewUser createUserPost);
        Task<UserDto[]> GetUsersAsync();

    }
}
