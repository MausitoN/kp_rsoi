using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionService.UserDb
{
    public class User : IdentityUser
    {
        public Guid UserUid { get; set; }
    }
}
