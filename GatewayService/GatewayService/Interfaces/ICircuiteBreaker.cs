using GatewayService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Interfaces
{
    public interface ICircuiteBreaker
    {
        Task<ServiceResponseGateway> ExecuteActionAsync(Func<Task<ServiceResponseGateway>> action);
    }
}
