using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareService.Configurations
{
    public class ServiceCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class ServicesCredentialsOptions
    {
        public ServiceCredentials[] Credentials { get; set; }
    }
}
