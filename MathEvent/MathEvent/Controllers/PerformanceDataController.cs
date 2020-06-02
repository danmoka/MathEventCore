using System;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathEvent.Helpers;
using System.IO;
using System.Net;
using MathEvent.Helpers.Access;

namespace MathEvent.Controllers
{
    /// <summary>
    /// API контроллер действий с событиями
    /// Необходим для обработки запросов с компонентов
    /// </summary>
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

        /// <summary>
        /// Изменяет данные о событии
        /// </summary>
        /// <param name="performanceModel">Вью-модель события</param>
        /// <returns>Статус код результата обработки</returns>
        [HttpPost]
        [Route("edit")]
        public async Task<HttpStatusCode> EditPerformance(
            [Bind("Id", "UserId", "Name", "Type", "Location", "KeyWords", "Annotation", "Start", "SectionId", "IsSectionData")] PerformanceViewModel performanceModel)
        {
            if (!await _userService.IsPerformanceModifier(performanceModel.Id, performanceModel.UserId))
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
            performance.Location = performanceModel.Location;
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

        /// <summary>
        /// Создает новое событие
        /// </summary>
        /// <param name="performanceModel">Вью-модель события</param>
        /// <returns>Вью-модель события с идентификатором добавленного события, 
        ///             с целью загрузки афиши</returns>
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

            if (performanceModel.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performanceModel.SectionId).SingleOrDefaultAsync();

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

        /// <summary>
        /// Удаляет событие
        /// </summary>
        /// <param name="performanceModel">Вью-модель события</param>
        /// <returns>Статус код результата обработки</returns>
        [HttpPost]
        [Route("delete")]
        public async Task<HttpStatusCode> DeletePerformance(
            [Bind("Id", "UserId")] PerformanceViewModel performanceModel)
        {
            if (!await _userService.IsPerformanceModifier(performanceModel.Id, performanceModel.UserId))
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

            if (!UserDataPathWorker.RemoveDirectory(path))
            {
                return HttpStatusCode.NotFound;
            }

            return HttpStatusCode.OK;
        }
    }
}