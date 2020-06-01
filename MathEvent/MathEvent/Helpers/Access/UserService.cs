using MathEvent.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MathEvent.Helpers.Access
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;
        public UserService(UserManager<ApplicationUser> userManager, ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<bool> IsAdmin(string userId)
        {
            var users = await _userManager.GetUsersInRoleAsync("admin");

            return users.Select(u => u.Id).Contains(userId);
        }

        public async Task<string> GetUserDataPath(string userId)
        {
            var user = await _db.Users.FindAsync(userId);

            if (user != null)
            {
                return user.DataPath;
            }

            return string.Empty;
        }

        public async Task<List<Conference>> GetUserConferences(string managerId)
        {
            if (await IsAdmin(managerId))
            {
                return await _db.Conferences.ToListAsync();
            }

            return await _db.Conferences.Where(c => c.ManagerId == managerId).ToListAsync();
        }

        public async Task<bool> IsConferenceModifier(int conferenceId, string userId)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefaultAsync();

            var isModifier = false;

            if (conference == null)
            {
                return isModifier;
            }

            if (conference.ManagerId == userId)
            {
                isModifier |= true;
            }

            if (await IsAdmin(userId))
            {
                isModifier |= true;
            }

            return isModifier;
        }

        public async Task<bool> IsPerformanceModifier(int performanceId, string userId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(s => s.Section)
                .SingleOrDefaultAsync();

            var isModifier = false;

            if (performance == null)
            {
                return isModifier;
            }

            if (performance.CreatorId == userId)
            {
                isModifier |= true;
            }

            if (performance.Section != null &&
                performance.Section.ManagerId == userId)
            {
                isModifier |= true;
            }

            if (await IsAdmin(userId))
            {
                isModifier |= true;
            }

            return isModifier;
        }

        public async Task<bool> IsSectionModifier(int sectionId, string userId)
        {
            var section = await _db.Sections.Where(s => s.Id == sectionId)
                .Include(c => c.Conference)
                .SingleOrDefaultAsync();

            var isModifier = false;

            if (section == null)
            {
                return isModifier;
            }

            if (section.ManagerId == userId)
            {
                isModifier |= true;
            }

            if (section.Conference != null &&
                section.Conference.ManagerId == userId)
            {
                isModifier |= true;
            }

            if (await IsAdmin(userId))
            {
                isModifier |= true;
            }

            return isModifier;
        }
    }
}
