using BOs.Models;
using Repos;

namespace Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepo _reportRepository;

        public ReportService(IReportRepo reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public Task<Report> CreateReportAsync(Report report)
        {
            return _reportRepository.CreateReportAsync(report);
        }

        public Task<bool> DeleteReportAsync(int reportId)
        {
            return _reportRepository.DeleteReportAsync(reportId);
        }

        public Task<List<Report>> GetAllReportsAsync()
        {
            return _reportRepository.GetAllReportsAsync();
        }

        public Task<Report?> GetReportByIdAsync(int reportId)
        {
            return _reportRepository.GetReportByIdAsync(reportId);
        }

        public Task<bool> UpdateReportAsync(Report report)
        {
            return _reportRepository.UpdateReportAsync(report);
        }
    }
}
