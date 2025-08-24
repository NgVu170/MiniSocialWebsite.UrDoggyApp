using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<Report>> GetAllReports();
        Task<int> GetCount();
        Task AddReport(Report report);
        Task DeleteReport(int reportId);
        Task DeleteReportsForPost(int postId);
        Task<Report> GetReportById(int reportId);
    }
}
