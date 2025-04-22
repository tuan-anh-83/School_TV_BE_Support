using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepo _programRepository;

        public ProgramService(IProgramRepo programRepository)
        {
            _programRepository = programRepository;
        }

        public async Task<List<Program>> GetProgramsWithVideosAsync()
        {
            return await _programRepository.GetProgramsWithVideoHistoryAsync();
        }

        public async Task<List<Program>> GetProgramsWithoutVideosAsync()
        {
            return await _programRepository.GetProgramsWithoutVideoHistoryAsync();
        }

        public async Task<IEnumerable<Program>> GetProgramsByChannelIdAsync(int channelId)
        {
            return await _programRepository.GetProgramsByChannelIdWithIncludesAsync(channelId);
        }

        public async Task<Program> CreateProgramAsync(Program program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            if (string.IsNullOrWhiteSpace(program.ProgramName))
                throw new ArgumentException("Program name is required.");

            return await _programRepository.CreateProgramAsync(program);
        }

        public async Task<Program?> GetProgramByIdAsync(int programId)
        {
            if (programId <= 0)
                throw new ArgumentException("Invalid Program ID.");

            return await _programRepository.GetProgramByIdAsync(programId);
        }

        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await _programRepository.GetAllProgramsAsync();
        }

        public async Task<bool> UpdateProgramAsync(Program program)
        {
            return await _programRepository.UpdateProgramAsync(program);
        }

        public async Task<bool> DeleteProgramAsync(int programId)
        {
            return await _programRepository.DeleteProgramAsync(programId);
        }

        public async Task<IEnumerable<Program>> SearchProgramsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search name cannot be empty.");

            return await _programRepository.SearchProgramsByNameAsync(name);
        }

        public async Task<int> CountProgramsAsync()
        {
            return await _programRepository.CountProgramsAsync();
        }

        public async Task<int> CountProgramsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty.");

            return await _programRepository.CountProgramsByStatusAsync(status);
        }

        public async Task<int> CountProgramsByScheduleAsync(int scheduleId)
        {
            if (scheduleId <= 0)
                throw new ArgumentException("Invalid Schedule ID.");

            return await _programRepository.CountProgramsByScheduleAsync(scheduleId);
        }
    }
}
