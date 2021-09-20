using SessionService.Models;
using SessionService.UserDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionService.Converters
{
    public class CreateUserPostDtoConverter
    {
        public static UserDb.User ConvertBack(NewUser createUserPost, Guid userUid)
        {
            if (createUserPost == null)
                return null;

            return new UserDb.User()
            {
                UserName = createUserPost.Login,
                UserUid = userUid
            };
        }
    }
}
