using Microsoft.EntityFrameworkCore;
using ReportService.Interfaces;
using ReportService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReportService.Services
{
    public class ReportsService : IReportService
    {
        private readonly ReportContext _context;
        private readonly IQueueRabbitMQ _queueRabbitMQ;

        public ReportsService(ReportContext context,
                              IQueueRabbitMQ queueRabbitMQ)
        {
            _context = context;
            _queueRabbitMQ = queueRabbitMQ;
        }

        public async Task<ServiceResponseReport> CheckQueue()
        {
            try
            {
                var response = await _queueRabbitMQ.Consume();

                return new ServiceResponseReport(response);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseReport(500, e.Message);
            }
        }
        public async Task<ServiceResponseReport> AddReport(ReportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new ServiceResponseReport(400, new ErrorResponse { Message = "Bad request format" });
                }

                var report = new Report();
                report.Model = request.Model;
                report.Location = request.Location;

                await _context.Reports.AddAsync(report);
                await _context.SaveChangesAsync();

                return new ServiceResponseReport(201);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseReport(500, e.Message);
            }
        }

        public async Task<ServiceResponseReport> GetModel()
        {
            try
            {
                var response = await CheckQueue();

                IEnumerable<Report> reports = await _context.Reports
                        .AsNoTracking()
                        .ToListAsync();

                if (reports == null)
                {
                    return new ServiceResponseReport(404, new ErrorResponse { Message = "Report not found" });
                }

                List<ModelReport> ReportResponse = new List<ModelReport>();
                bool flag;
                foreach (Report report in reports)
                {
                    flag = false;
                    foreach (ModelReport mr in ReportResponse)
                    {
                        if (mr.Model == report.Model)
                        {
                            flag = true;
                            mr.ModelCount += 1;
                        }
                    }

                    if (flag == false)
                    {
                        ReportResponse.Add(new ModelReport
                        {
                            Model = report.Model,
                            ModelCount = 1
                        });
                    }
                }
                return new ServiceResponseReport(ReportResponse);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseReport(500, e.Message);
            }
        }

        public async Task<ServiceResponseReport> GetLocation()
        {
            try
            {
                var response = await CheckQueue();

                IEnumerable<Report> reports = await _context.Reports
                        .AsNoTracking()
                        .ToListAsync();

                if (reports == null)
                {
                    return new ServiceResponseReport(404, new ErrorResponse { Message = "Report not found" });
                }

                List<LocationReport> ReportResponse = new List<LocationReport>();
                bool flag;
                foreach (Report report in reports)
                {
                    flag = false;
                    foreach (LocationReport rr in ReportResponse)
                    {
                        if (rr.Location == report.Location)
                        {
                            flag = true;
                            rr.LocationCount += 1;
                        }
                    }

                    if (flag == false)
                    {
                        ReportResponse.Add(new LocationReport
                        {
                            Location = report.Location,
                            LocationCount = 1
                        });
                    }
                }
                return new ServiceResponseReport(ReportResponse);
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new ServiceResponseReport(500, e.Message);
            }
        }
    }
}
