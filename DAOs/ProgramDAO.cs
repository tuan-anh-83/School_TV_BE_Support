using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class ProgramDAO
    {

        private static ProgramDAO instance = null;
        private readonly DataContext _context;

        private ProgramDAO()
        {
            _context = new DataContext();
        }

        public static ProgramDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProgramDAO();
                }
                return instance;
            }
        }

        public async Task<List<Program>> GetProgramsByChannelIdWithIncludesAsync(int channelId)
        {
            return await _context.Programs
                .Where(p => p.SchoolChannelID == channelId)
                .Include(p => p.SchoolChannel)
                .Include(p => p.VideoHistories)
                .Include(p => p.ProgramFollows)
                .Include(p => p.Schedules)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Program>> GetProgramsWithVideoHistoryAsync()
        {
            return await _context.Programs
                .Where(p => p.VideoHistories.Any())
                .ToListAsync();
        }

        public async Task<List<Program>> GetProgramsWithoutVideoHistoryAsync()
        {
            return await _context.Programs
                .Where(p => !p.VideoHistories.Any())
                .ToListAsync();
        }

        public async Task<IEnumerable<Program>> GetProgramsByChannelIdAsync(int channelId)
        {
            return await _context.Programs
                .Where(p => p.SchoolChannelID == channelId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await _context.Programs
                .Include(p => p.SchoolChannel)
                .Include(p => p.Schedules)
                .Include(p => p.VideoHistories)
                .Include(p => p.ProgramFollows)
                .ToListAsync();
        }

        public async Task<Program?> GetProgramByIdAsync(int programId)
        {
            if (programId <= 0)
                throw new ArgumentException("Program ID must be greater than zero.");

            return await _context.Programs
                .Include(p => p.SchoolChannel)
                .Include(p => p.Schedules)
                .Include(p => p.VideoHistories)
                .Include(p => p.ProgramFollows)
                .FirstOrDefaultAsync(p => p.ProgramID == programId);
        }

        public async Task<IEnumerable<Program>> SearchProgramsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Program name cannot be null or empty.");

            return await _context.Programs
                .Include(p => p.SchoolChannel)
                .Include(p => p.Schedules)
                .Include(p => p.VideoHistories)
                .Include(p => p.ProgramFollows)
                .Where(p => EF.Functions.Like(p.ProgramName, $"%{name}%"))
                .ToListAsync();
        }

        public async Task<Program> CreateProgramAsync(Program program)
        {
            if (program == null)
                throw new ArgumentNullException(nameof(program));

            var existingSchoolChannel = await _context.SchoolChannels.FindAsync(program.SchoolChannelID);
            if (existingSchoolChannel == null)
                throw new InvalidOperationException("Invalid SchoolChannelID.");

            program.SchoolChannel = existingSchoolChannel;

            await _context.Programs.AddAsync(program);
            await _context.SaveChangesAsync();

            return program;
        }

        public async Task<bool> UpdateProgramAsync(Program program)
        {
            if (program == null || program.ProgramID <= 0)
                throw new ArgumentException("Invalid Program data.");

            var existingProgram = await _context.Programs
                                        .Include(p => p.Schedules)
                                        .FirstOrDefaultAsync(p => p.ProgramID == program.ProgramID);
            if (existingProgram == null)
                throw new InvalidOperationException("Program not found.");

            bool isBeingDeactivated = !string.Equals(existingProgram.Status, "Inactive", StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(program.Status, "Inactive", StringComparison.OrdinalIgnoreCase);

            _context.Entry(existingProgram).CurrentValues.SetValues(program);

            if (isBeingDeactivated && existingProgram.Schedules != null)
            {
                foreach (var schedule in existingProgram.Schedules)
                {
                    schedule.Status = "Inactive";
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProgramAsync(int programId)
        {
            if (programId <= 0)
                throw new ArgumentException("Program ID must be greater than zero.");

            var program = await _context.Programs
                              .Include(p => p.Schedules)
                              .FirstOrDefaultAsync(p => p.ProgramID == programId);
            if (program == null)
                return false;

            program.Status = "Inactive";
            program.UpdatedAt = DateTime.UtcNow;

            if (program.Schedules != null)
            {
                foreach (var schedule in program.Schedules)
                {
                    schedule.Status = "Inactive";
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountProgramsAsync()
        {
            return await _context.Programs.CountAsync();
        }

        public async Task<int> CountProgramsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty.");

            return await _context.Programs.CountAsync(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<int> CountProgramsByScheduleAsync(int scheduleId)
        {
            if (scheduleId <= 0)
                throw new ArgumentException("Schedule ID must be greater than zero.");

            return await _context.Programs
                .Include(p => p.Schedules)
                .Where(p => p.Schedules.Any(s => s.ScheduleID == scheduleId))
                .CountAsync();
        }

    }
}
