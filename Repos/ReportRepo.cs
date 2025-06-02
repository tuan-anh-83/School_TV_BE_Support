using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class ReportRepo : IReportRepo
    {
        public async Task<Report> CreateReportAsync(Report report)
        {
            return await ReportDAO.Instance.CreateReportAsync(report);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            return await ReportDAO.Instance.DeleteReportAsync(reportId);
        }

        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await ReportDAO.Instance.GetAllReportAsync();
        }

        public async Task<Report?> GetReportByIdAsync(int reportId)
        {
            return await ReportDAO.Instance.GetReportByIdAsync(reportId);
        }

        public async Task<bool> UpdateReportAsync(Report report)
        {
            return await ReportDAO.Instance.UpdateReportAsync(report);
        }
    }
}
