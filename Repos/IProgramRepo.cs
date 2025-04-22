using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IProgramRepo
    {
        Task<Program> CreateProgramAsync(Program program);
        Task<Program?> GetProgramByIdAsync(int programId);
        Task<IEnumerable<Program>> GetAllProgramsAsync();
        Task<bool> UpdateProgramAsync(Program program);
        Task<bool> DeleteProgramAsync(int programId);
        Task<IEnumerable<Program>> SearchProgramsByNameAsync(string name);
        Task<int> CountProgramsAsync();
        Task<int> CountProgramsByStatusAsync(string status);
        Task<int> CountProgramsByScheduleAsync(int scheduleId);
        Task<IEnumerable<Program>> GetProgramsByChannelIdAsync(int channelId);
        Task<List<Program>> GetProgramsWithVideoHistoryAsync();
        Task<List<Program>> GetProgramsWithoutVideoHistoryAsync();
        Task<List<Program>> GetProgramsByChannelIdWithIncludesAsync(int channelId);
    }
}
