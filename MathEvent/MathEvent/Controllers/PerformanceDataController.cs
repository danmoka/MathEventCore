using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathEvent.Helpers;
using System.IO;
using System.Net;
using MathEvent.Helpers.Access;

namespace MathEvent.Controllers
{
    [Route("api/performances")]
    [ApiController]
    public class PerformanceDataController : Controller
    {
        private readonly ApplicationContext _db;
        private readonly UserService _userService;

        public PerformanceDataController(ApplicationContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        [HttpPost]
        [Route("edit")]
        public async Task<HttpStatusCode> EditPerformance(
            [Bind("Id", "UserId", "Name", "Type", "Location", "KeyWords", "Annotation", "Start", "SectionId", "IsSectionData")] PerformanceViewModel performanceModel)
        {
            if (!await IsPerformanceModifier(performanceModel.Id, performanceModel.UserId))
            {
                return HttpStatusCode.Forbidden;
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceModel.Id).SingleOrDefaultAsync();

            if (performance == null)
            {
                return HttpStatusCode.NotFound;
            }

            performance.Type = performanceModel.Type;
            performance.Name = performanceModel.Name;
            performance.Annotation = performanceModel.Annotation;
            performance.KeyWords = performanceModel.KeyWords;
            performance.Start = performanceModel.Start;
            //performance.CreatorId = performanceModel.UserId;
            performance.Location = performanceModel.Location;
            
            //performance.DataPath = performanceModel.DataPath;
            performance.IsSectionData = performanceModel.IsSectionData;


            if (performance.SectionId != performanceModel.SectionId)
            {
                var performanceDataPath = performance.DataPath;

                var newDataPath = performance.DataPath;

                if (performanceModel.SectionId != null)
                {
                    var section = await _db.Sections.Where(s => s.Id == performanceModel.SectionId).SingleOrDefaultAsync();

                    if (section == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    newDataPath = UserDataPathWorker.ConcatPaths(section.DataPath, performance.Id.ToString());
                }
                else
                {
                    var userDataPath = await _userService.GetUserDataPath(performanceModel.UserId);

                    if (string.IsNullOrEmpty(userDataPath))
                    {
                        return HttpStatusCode.NotFound;
                    }

                    newDataPath = UserDataPathWorker.ConcatPaths(userDataPath, performance.Id.ToString());
                }

                var newRootDataPath = UserDataPathWorker.GetRootPath(newDataPath);
                var performanceRootDataPath = UserDataPathWorker.GetRootPath(performanceDataPath);

                if(!UserDataPathWorker.MoveDirectories(performanceRootDataPath, newRootDataPath))
                {
                    return HttpStatusCode.InternalServerError;
                }
                

                performance.DataPath = newDataPath;
                performance.SectionId = performanceModel.SectionId;
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return HttpStatusCode.OK;
        }

        [HttpPost]
        [Route("create")]
        public async Task<PerformanceViewModel> CreatePerformance(
            [Bind("UserId", "Name", "Type", "Location", "KeyWords", "Annotation", "Start", "SectionId", "IsSectionData")] PerformanceViewModel performanceModel)
        {
            var performance = new Performance
            {
                Name = performanceModel.Name,
                Type = performanceModel.Type,
                KeyWords = performanceModel.KeyWords,
                Annotation = performanceModel.Annotation,
                CreatorId = performanceModel.UserId,
                Start = performanceModel.Start,
                Location = performanceModel.Location,
                SectionId = performanceModel.SectionId,
                IsSectionData = performanceModel.IsSectionData
            };

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).SingleOrDefaultAsync();

                if (section == null)
                {
                    throw new ArgumentNullException("ops...");
                }

                performance.DataPath = section.DataPath;
            }
            else
            {
                var userDataPath = await _userService.GetUserDataPath(performanceModel.UserId);

                if (string.IsNullOrEmpty(userDataPath))
                {
                    throw new ArgumentNullException("ops...");
                }

                performance.DataPath = userDataPath;
            }

            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath = performance.DataPath;

            if (!UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString()))
            {
                _db.Performances.Remove(performance);
                await _db.SaveChangesAsync();

                throw new Exception("ops...");
            }

            performance.DataPath = performanceDataPath;
            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            performanceModel.Id = performance.Id;

            return performanceModel;
        }

        [HttpPost]
        [Route("delete")]
        public async Task<HttpStatusCode> DeletePerformance(
            [Bind("Id", "UserId")] PerformanceViewModel performanceModel)
        {
            if (!await IsPerformanceModifier(performanceModel.Id, performanceModel.UserId))
            {
                return HttpStatusCode.Forbidden;
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceModel.Id).SingleOrDefaultAsync();

            if (performance == null)
            {
                return HttpStatusCode.NotFound;
            }

            var path = UserDataPathWorker.GetRootPath(performance.DataPath);

            _db.Performances.Remove(performance);
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

        private async Task<bool> IsPerformanceModifier(int performanceId, string userId)
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

            if (await _userService.IsAdmin(userId))
            {
                isModifier |= true;
            }

            return isModifier;
        }
    }
}