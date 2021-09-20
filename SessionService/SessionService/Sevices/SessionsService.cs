using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SessionService.Converters;
using SessionService.Interfaces;
using SessionService.Models;
using SessionService.UserDb;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SessionService.Sevices
{
    public class SessionsService : ISessionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserDb.User> _userManager;

        public SessionsService(AppDbContext context,
                               UserManager<UserDb.User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Task<ServiceResponseSession> AddUser(NewUser request)
        {
            throw new NotImplementedException();
        }

        /*public async Task<ServiceResponseSession> GetUsers()
        {
            try
            {
                IEnumerable<User> users = await _context.Users
                    .AsNoTracking()
                    .ToListAsync();

                if (users == null)
                {
                    return new ServiceResponseSession(404, new ErrorResponse { Message = "Item info not found" });
                }

                return new ServiceResponseSession(users);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseSession(500, e.Message);
            }
        }

        public async Task<ServiceResponseSession> AddUser(NewUser request)
        {
            if (request == null)
            {
                return new ServiceResponseSession(400, new ErrorResponse { Message = "Bad request format" });
            }

            UnicodeEncoding uEncode = new UnicodeEncoding();
            SHA512Managed sha = new SHA512Managed();
            var hashPassword = Convert.ToBase64String(sha.ComputeHash(uEncode.GetBytes(request.Password)));

            var user = new User();
            user.Id = Guid.NewGuid(); ;
            user.Login = request.Login;
            user.Password = hashPassword;
            user.Role = "User";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return new ServiceResponseSession(200);
        }*/
        /*
        public async Task<ServiceResponseSession> Authorization(NewUser request)
        {
            var identity = GetIdentity(request.Login, request.Password);
            if (identity == null)
            {
                return new ServiceResponseSession(400, new ErrorResponse { Message = "Invalid username or password.t" });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);


            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return new ServiceResponseSession(response);
        }
        */
        /*public async Task<Models.User> GetIdentity(string username, string password)
        {
            UnicodeEncoding uEncode = new UnicodeEncoding();
            SHA512Managed sha = new SHA512Managed();
            var hashPassword = Convert.ToBase64String(sha.ComputeHash(uEncode.GetBytes(password)));
            
            Models.User user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Login == username && u.Password == hashPassword)
                    .SingleOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return user;
        }*/

        public async Task<UserDto> CreateUserAsync(NewUser createUserPost)
        {
            var user = CreateUserPostDtoConverter.ConvertBack(createUserPost, Guid.NewGuid());
            var result = await _userManager.CreateAsync(user, createUserPost.Password);

            if (result.Succeeded)
            {
                var newUser = await _userManager.FindByNameAsync(user.UserName);
                await _userManager.AddToRoleAsync(newUser, "common");
                return UserDtoConverter.Convert(newUser, "common");
            }
            else
            {
                throw new Exception();
            }
        }

        public Task<Models.User> GetIdentity(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseSession> GetUsers()
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto[]> GetUsersAsync()
        {
            var users = _userManager.Users.ToArray();
            var usersDto = new UserDto[users.Length];
            for (int i = 0; i < usersDto.Length; i++)
            {
                var roles = await _userManager.GetRolesAsync(users[i]);
                usersDto[i] = UserDtoConverter.Convert(users[i], roles.FirstOrDefault());
            }

            return usersDto;
        }
    }
}
