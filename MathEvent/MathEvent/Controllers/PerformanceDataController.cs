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

namespace MathEvent.Controllers
{
    [Route("api/performances")]
    [ApiController]
    public class PerformanceDataController : Controller
    {
        private readonly ApplicationContext _db;

        public PerformanceDataController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Route("edit")]
        public async Task<string> EditPerformance(PerformanceViewModel performanceModel)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceModel.Id).SingleOrDefaultAsync();

            if (performance != null)
            {
                performance.Type = performanceModel.Type;
                performance.Name = performanceModel.Name;
                performance.Annotation = performanceModel.Annotation;
                performance.KeyWords = performanceModel.KeyWords;
                performance.Start = performanceModel.Start;
                performance.CreatorId = performanceModel.UserId;
                performance.Location = performanceModel.Location;
                performance.SectionId = performanceModel.SectionId;
                performance.DataPath = performanceModel.DataPath;

                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();

                return string.Empty;
            }

            return "Не удалось обновить событие";
        }

        [HttpPost]
        [Route("create")]
        public async Task<PerformanceViewModel> CreatePerformance(PerformanceViewModel performanceModel)
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
                SectionId = performanceModel.SectionId
            };

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).SingleOrDefaultAsync();

                if (section != null)
                {
                    performance.DataPath = section.DataPath;
                }
                else
                {
                    //хотя такого не должно быть
                    performance.DataPath = performanceModel.UserDataPath;
                }
            }
            else
            {
                performance.DataPath = performanceModel.UserDataPath;
            }

            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath = performance.DataPath;

            if (!UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString()))
            {
                _db.Performances.Remove(performance);
                await _db.SaveChangesAsync();

                //ошибка
            }

            performance.DataPath = performanceDataPath;
            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            performanceModel.Id = performance.Id;

            return performanceModel;
        }
    }
}