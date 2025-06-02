using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IReportService
    {
        Task<List<Report>> GetAllReportsAsync();
        Task<Report?> GetReportByIdAsync(int reportId);
        Task<Report> CreateReportAsync(Report report);
        Task<bool> UpdateReportAsync(Report report);
        Task<bool> DeleteReportAsync(int reportId);
    }
}
