using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareService.Configurations
{
    public class SecretOptions
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public int AccessExpirationInMin { get; set; }
    }
}
