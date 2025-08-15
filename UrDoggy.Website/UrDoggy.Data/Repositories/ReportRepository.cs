using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models; // Report

namespace UrDoggy.Data.Repositories
{
    public class ReportRepository
    {
        private readonly ApplicationDbContext _context;
        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetCount()
        {
            return await _context.Reports.AsNoTracking().CountAsync();
        }

        public async Task<List<Report>> GetAll()
        {
            return await _context.Reports
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task Add (Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int reportId)
        {
            await _context.Reports.Where(r => r.Id == reportId)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteByPostId(int postId)
        {
            await _context.Reports.Where(r => r.PostId == postId)
                .ExecuteDeleteAsync();
        }
    }
}
