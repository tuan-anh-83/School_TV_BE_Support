using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace DAOs
{
    public class ReportDAO
    {

        private static ReportDAO instance = null;
        private readonly DataContext _context;

        private ReportDAO()
        {
            _context = new DataContext();
        }

        public static ReportDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ReportDAO();
                }
                return instance;
            }
        }

        public async Task<List<Report>> GetAllReportAsync()
        {
            return await _context.Reports.AsNoTracking().ToListAsync();
        }

        public async Task<Report?> GetReportByIdAsync(int reportId)
        {
            if (reportId <= 0)
                throw new ArgumentException("Program ID must be greater than zero.");

            return await _context.Reports.FindAsync(reportId);
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<bool> UpdateReportAsync(Report report)
        {
            if (report == null || report.ReportID <= 0)
                throw new ArgumentException("Invalid Report data.");

            var existingReport = await _context.Reports.FindAsync(report.ReportID);
            if (existingReport == null)
                throw new InvalidOperationException("Report not found.");

            _context.Entry(existingReport).CurrentValues.SetValues(report);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            if (reportId <= 0)
                throw new ArgumentException("Repport ID must be greater than zero.");

            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return false;

            _context.Reports.Remove(report);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
