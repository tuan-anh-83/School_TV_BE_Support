using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class ProgramRepo : IProgramRepo
    {
        public async Task<int> CountProgramsAsync()
        {
            return await ProgramDAO.Instance.CountProgramsAsync();
        }

        public async Task<int> CountProgramsByScheduleAsync(int scheduleId)
        {
            return await ProgramDAO.Instance.CountProgramsByScheduleAsync(scheduleId);
        }

     

        public async Task<int> CountProgramsByStatusAsync(string status)
        {
            return await ProgramDAO.Instance.CountProgramsByStatusAsync(status);
        }

        public async Task<Program> CreateProgramAsync(Program program)
        {
            return await ProgramDAO.Instance.CreateProgramAsync(program);
        }

        public async Task<bool> DeleteProgramAsync(int programId)
        {
            return await ProgramDAO.Instance.DeleteProgramAsync(programId);
        }

        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await ProgramDAO.Instance.GetAllProgramsAsync();
        }

        public async Task<Program?> GetProgramByIdAsync(int programId)
        {
            return await ProgramDAO.Instance.GetProgramByIdAsync(programId);
        }

        public async Task<IEnumerable<Program>> GetProgramsByChannelIdAsync(int channelId)
        {
            return await ProgramDAO.Instance.GetProgramsByChannelIdAsync(channelId);
        }

        public async Task<List<Program>> GetProgramsByChannelIdWithIncludesAsync(int channelId)
        {
            return await ProgramDAO.Instance.GetProgramsByChannelIdWithIncludesAsync(channelId);
        }

        public async Task<List<Program>> GetProgramsWithoutVideoHistoryAsync()
        {
            return await ProgramDAO.Instance.GetProgramsWithoutVideoHistoryAsync();
        }

        public async Task<List<Program>> GetProgramsWithVideoHistoryAsync()
        {
            return await ProgramDAO.Instance.GetProgramsWithVideoHistoryAsync();
        }

        public async Task<IEnumerable<Program>> SearchProgramsByNameAsync(string name)
        {
            return await ProgramDAO.Instance.SearchProgramsByNameAsync(name);
        }

        public async Task<bool> UpdateProgramAsync(Program program)
        {
            return await ProgramDAO.Instance.UpdateProgramAsync(program);
        }
    }
}
