using SessionService.UserDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionService.Converters
{
    public class UserDtoConverter
    {
        public static UserDto Convert(User user, string role)
        {
            if (user == null)
                return null;

            return new UserDto()
            {
                Login = user.UserName,
                Role = role,
                UserUid = user.UserUid
            };
        }
    }
}
