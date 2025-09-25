using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<HashSet<int> GetAllRankCalculate(int currentUserId);
        Task
    }
}
