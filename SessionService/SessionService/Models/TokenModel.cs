using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SessionService.Models
{
    public class TokenModel
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
