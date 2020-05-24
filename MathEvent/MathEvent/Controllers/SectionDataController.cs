using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Route("api/sections")]
    [ApiController]
    public class SectionDataController : ControllerBase
    {
        private readonly ApplicationContext _db;

        public SectionDataController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Route("edit")]
        public async Task<HttpStatusCode> EditSection(SectionViewModel sectionModel)
        {
            if (!await IsSectionModifier(sectionModel.Id, sectionModel.UserId, sectionModel.UserRoles))
            {
                return HttpStatusCode.Forbidden;
            }

            var section = await _db.Sections.Where(s => s.Id == sectionModel.Id).SingleOrDefaultAsync();

            if (section == null)
            {
                return HttpStatusCode.NotFound;
            }

            section.Name = sectionModel.Name;
            section.Location = sectionModel.Location;
            section.Start = sectionModel.Start;
            section.End = sectionModel.End;
            section.ConferenceId = sectionModel.ConferenceId;
            //section.ManagerId = sectionModel.UserId; // надо ли?
            //section.DataPath = sectionModel.DataPath;
            _db.Sections.Update(section);

            var performances = await _db.Performances.Where(p => p.SectionId == section.Id && p.IsSectionData).ToListAsync();

            if (performances == null)
            {
                return HttpStatusCode.NotFound;
            }

            foreach (var performance in performances)
            {
                performance.Location = section.Location;
                performance.Start = section.Start;
            }

            _db.Performances.UpdateRange(performances);

            await _db.SaveChangesAsync();

            return HttpStatusCode.OK;
        }

        [HttpPost]
        [Route("delete")]
        public async Task<HttpStatusCode> DeleteSection(SectionViewModel sectionModel)
        {
            if (!await IsSectionModifier(sectionModel.Id, sectionModel.UserId, sectionModel.UserRoles))
            {
                return HttpStatusCode.Forbidden;
            }

            var section = await _db.Sections.Where(s => s.Id == sectionModel.Id).SingleOrDefaultAsync();

            if (section == null)
            {
                return HttpStatusCode.NotFound;
            }

            var path = UserDataPathWorker.GetRootPath(section.DataPath);

            _db.Sections.Remove(section);
            await _db.SaveChangesAsync();

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    return HttpStatusCode.NotFound;
                }

            }

            return HttpStatusCode.OK;
        }

        private async Task<bool> IsSectionModifier(int sectionId, string userId, List<string> userRoles)
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

            foreach (var userRole in userRoles)
            {
                if (userRole == "admin")
                {
                    isModifier |= true;
                    break;
                }
            }

            return isModifier;
        }
    }
}