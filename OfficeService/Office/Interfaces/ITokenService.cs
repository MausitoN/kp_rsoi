using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Interfaces
{
    public interface ITokenService
    {
        string GetCarServiceToken();
        void SetCarServiceToken(string token);
    }
}
