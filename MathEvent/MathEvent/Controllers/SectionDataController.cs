using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Helpers.Access;
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
        private readonly UserService _userService;

        public SectionDataController(ApplicationContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        [HttpPost]
        [Route("edit")]
        public async Task<HttpStatusCode> EditSection(
            [Bind("Id", "UserId", "Name", "Location", "Start", "End", "ConferenceId")] SectionViewModel sectionModel)
        {
            if (!ModelState.IsValid)
            {
                return HttpStatusCode.BadRequest;
            }

            if (!await IsSectionModifier(sectionModel.Id, sectionModel.UserId))
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

            if (section.ConferenceId != sectionModel.ConferenceId)
            {
                var sectionDataPath = section.DataPath;
                var newDataPath = section.DataPath;
                var conference = await _db.Conferences.Where(c => c.Id == sectionModel.ConferenceId).SingleOrDefaultAsync();

                if (conference == null)
                {
                    return HttpStatusCode.NotFound;
                }

                newDataPath = UserDataPathWorker.ConcatPaths(conference.DataPath, section.Id.ToString());

                var newRootDataPath = UserDataPathWorker.GetRootPath(newDataPath);
                var sectionRootDataPath = UserDataPathWorker.GetRootPath(sectionDataPath);

                if (!UserDataPathWorker.MoveDirectories(sectionRootDataPath, newRootDataPath))
                {
                    return HttpStatusCode.InternalServerError;
                }

                section.DataPath = newDataPath;
                section.ConferenceId = sectionModel.ConferenceId;
                _db.Sections.Update(section);

                foreach (var performance in performances)
                {
                    performance.DataPath = UserDataPathWorker.ConcatPaths(newDataPath, performance.Id.ToString());
                }
            }


            _db.Performances.UpdateRange(performances);

            await _db.SaveChangesAsync();

            return HttpStatusCode.OK;
        }

        [HttpPost]
        [Route("delete")]
        public async Task<HttpStatusCode> DeleteSection(
            [Bind("Id", "UserId")] SectionViewModel sectionModel)
        {
            if (!ModelState.IsValid)
            {
                return HttpStatusCode.BadRequest;
            }

            if (!await IsSectionModifier(sectionModel.Id, sectionModel.UserId))
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

        private async Task<bool> IsSectionModifier(int sectionId, string userId)
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

            if (await _userService.IsAdmin(userId))
            {
                isModifier |= true;
            }

            return isModifier;
        }
    }
}