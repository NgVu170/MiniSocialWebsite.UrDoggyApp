using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Services.Service
{
    public class ReportService : IReportService
    {
        private readonly ReportRepository _reportRepository;

        public ReportService(ReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<Report>> GetAllReports()
        {
            return await _reportRepository.GetAll();
        }

        public async Task<int> GetCount()
        {
            return await _reportRepository.GetCount();
        }

        public async Task AddReport(Report report)
        {
            await _reportRepository.Add(report);
        }

        public async Task DeleteReport(int reportId)
        {
            await _reportRepository.DeleteAsync(reportId);
        }

        public async Task DeleteReportsForPost(int postId)
        {
            await _reportRepository.DeleteByPostId(postId);
        }

        public async Task<Report> GetReportById(int reportId)
        {
            var allReports = await _reportRepository.GetAll();
            return allReports.FirstOrDefault(r => r.Id == reportId);
        }
    }
}
