using ReportService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Interfaces
{
    public interface IReportService
    {
        Task<ServiceResponseReport> AddReport(ReportRequest request);
        Task<ServiceResponseReport> GetModel();
        Task<ServiceResponseReport> GetLocation();
        Task<ServiceResponseReport> CheckQueue();
    }
}
