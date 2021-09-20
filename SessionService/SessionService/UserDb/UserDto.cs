using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SessionService.UserDb
{
    public class UserDto
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("userUid")]
        public Guid UserUid { get; set; }
    }
}
