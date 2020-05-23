using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    public class PerformanceController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;
        private static IWebHostEnvironment _webHostEnvironment;

        public PerformanceController(ApplicationContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Performance> performances = await _db.Performances
                .Where(p => p.Start.Month >= DateTime.Now.Month)
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();

            var cards = new List<PerformanceViewModel>();

            foreach (var performance in performances)
            {
                var card = new PerformanceViewModel
                {
                    Id = performance.Id,
                    Name = performance.Name,
                    Annotation = performance.Annotation,
                    KeyWords = performance.KeyWords,
                    Start = performance.Start,
                    CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}",
                    DataPath = performance.DataPath,
                    PosterName = performance.PosterName,
                    Traffic = performance.Traffic,
                    Type = performance.Type,
                    Location = performance.Location,
                    IsSectionData = performance.IsSectionData
                };

                cards.Add(card);
            }

            return View(cards);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var performance = new PerformanceViewModel
            {
                UserId = user.Id,
                UserDataPath = user.DataPath
            };

            return View(performance);
        }

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Add(
        //    [Bind("Name", "Type", "Location", "KeyWords", "Annotation", "Start", "SectionId", "IsSectionStartAndLocation")] PerformanceViewModel model, IFormFile uploadedFile)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return RedirectToAction("Error400", "Error");
        //    }

        //    //if (!model.IsSectionStartAndLocation)
        //    //{
        //    //    if (model.Start == null || String.IsNullOrEmpty(model.Location))
        //    //    {
        //    //        ModelState.AddModelError(string.Empty, "Адрес или время начала не введены. Введите или выставьте флажок");
        //    //        var userSections = await _db.Sections.ToListAsync();
        //    //        ViewBag.Sections = new SelectList(userSections, "Id", "Name");
        //    //        var types = DataFactory.GetPerformanceTypes().GetValues()
        //    //            .Select(x => new SelectListItem { Text = x, Value = x })
        //    //            .ToList();
        //    //        ViewBag.Types = types;

        //    //        return View(model);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    if (model.SectionId == null)
        //    //    {
        //    //        ModelState.AddModelError(string.Empty, "Секция не выбрана. Выберите или снимите флажок");
        //    //        var userSections = await _db.Sections.ToListAsync();
        //    //        ViewBag.Sections = new SelectList(userSections, "Id", "Name");
        //    //        var types = DataFactory.GetPerformanceTypes().GetValues()
        //    //            .Select(x => new SelectListItem { Text = x, Value = x })
        //    //            .ToList();
        //    //        ViewBag.Types = types;

        //    //        return View(model);
        //    //    }
        //    //}

        //    //var user = await _userManager.GetUserAsync(User);

        //    //if (user == null)
        //    //{
        //    //    return RedirectToAction("Error500", "Error");
        //    //}

        //    //var performance = new Performance
        //    //{
        //    //    Name = model.Name,
        //    //    Type = model.Type,
        //    //    KeyWords = model.KeyWords,
        //    //    Annotation = model.Annotation,
        //    //    CreatorId = user.Id,
        //    //    Start = model.Start,
        //    //    Location = model.Location,
        //    //    SectionId = model.SectionId == -1 ? null : model.SectionId
        //    //};

        //    //if (model.SectionId != null)
        //    //{
        //    //    var section = await _db.Sections.Where(s => s.Id == performance.SectionId).SingleOrDefaultAsync();

        //    //    if (section == null)
        //    //    {
        //    //        return RedirectToAction("Error500", "Error");
        //    //    }

        //    //    performance.DataPath = section.DataPath;
        //    //}
        //    //else
        //    //{
        //    //    performance.DataPath = user.DataPath;
        //    //}

        //    //await _db.Performances.AddAsync(performance);
        //    //await _db.SaveChangesAsync();

        //    //string performanceDataPath = performance.DataPath;

        //    //if(!UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString()))
        //    //{
        //    //    _db.Performances.Remove(performance);
        //    //    await _db.SaveChangesAsync();

        //    //    return RedirectToAction("Error500", "Error");
        //    //}

        //    //performance.DataPath = performanceDataPath;

        //    //if (uploadedFile != null)
        //    //{
        //    //    performance.PosterName = Path.GetFileName(uploadedFile.FileName);
        //    //}
        //    //else
        //    //{
        //    //    performance.PosterName = Path.GetFileName(UserDataPathWorker.GetDefaultImagePath());
        //    //}

        //    //await UserDataPathWorker.UploadImage(uploadedFile, performance.DataPath, performance.PosterName);

        //    //await _db.SaveChangesAsync();

        //    return RedirectToAction("Index", "Performance");
        //}

        [HttpGet]
        public async Task<IActionResult> Card(int id)
        {
            var performance = await _db.Performances.Where(p => p.Id == id)
                .Include(p => p.Section)
                .Include(p => p.Creator).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var card = new PerformanceViewModel
            {
                Id = performance.Id,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                CreatorName = $"{performance.Creator.Name} {performance.Creator.Surname}",
                DataPath = performance.DataPath,
                PosterName = performance.PosterName,
                Traffic = performance.Traffic,
                Location = performance.Location,
                UserInfo = performance.Creator.Info,
                Type = performance.Type,
                SectionId = performance.SectionId,
                IsSectionData = performance.IsSectionData
            };

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId)
                    .Include(s => s.Manager)
                    .Include(s => s.Conference)
                    .SingleOrDefaultAsync();

                if (section != null)
                {
                    var sectionInfo = $"Событие находится на секции \"{section.Name}\" ({section.Start:dd/MM/yyyy HH:mm} - {section.End:dd/MM/yyyy HH:mm}). ";

                    if (section.Manager != null)
                    {
                        sectionInfo += $"Создатель секции - {section.Manager.Name} {section.Manager.Surname}. ";
                    }

                    if (section.Conference != null)
                    {
                        sectionInfo += $"Конференция \"{section.Conference.Name}\".";
                    }

                    card.SectionInfo = sectionInfo;
                }
            }

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == id && ap.ApplicationUserId == userId).SingleOrDefaultAsync();

                card.IsSubscribed = ap != null;
                card.UserId = user.Id;
                card.UserRoles = (List<string>) await _userManager.GetRolesAsync(user);
            }

            return View(card);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int performanceId)
        {
            var performance = await _db.Performances.Where(c => c.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            if (! await IsPerformanceModifier(performanceId))
            {
                return RedirectToAction("Index", "Home");
            }

            var performanceModel = new PerformanceViewModel
            {
                Id = performance.Id,
                Type = performance.Type,
                Name = performance.Name,
                Annotation = performance.Annotation,
                KeyWords = performance.KeyWords,
                Start = performance.Start,
                UserId = user.Id,
                UserRoles = (List<string>) await _userManager.GetRolesAsync(user),
                Location = performance.Location,
                SectionId = performance.SectionId,
                DataPath = performance.DataPath,
                IsSectionData = performance.IsSectionData
            };

            return View(performanceModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Location", "Type", "KeyWords", "Annotation", "Start", "SectionId",
            "CreatorId", "DataPath", "PosterName", "Traffic")] Performance performance, IFormFile uploadedFile, IFormFile uploadedProceedings)
        {
            //if (!ModelState.IsValid)
            //{
            //    return RedirectToAction("Error400", "Error");
            //}

            //var dbPerformance = await _db.Performances.Where(p => p.Id == performance.Id).SingleOrDefaultAsync();
            
            //if (dbPerformance == null)
            //{
            //    return RedirectToAction("Error500", "Error");
            //}

            //if (! await IsPerformanceModifier(dbPerformance.Id))
            //{
            //    return RedirectToAction("Error500", "Error");
            //}

            //if (uploadedFile != null)
            //{
            //    var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(dbPerformance.DataPath, dbPerformance.PosterName));

            //    if (System.IO.File.Exists(imageToBeDeleted))
            //    {
            //        System.IO.File.Delete(imageToBeDeleted);
            //    }

            //    dbPerformance.PosterName = Path.GetFileName(uploadedFile.FileName);
            //    await UserDataPathWorker.UploadImage(uploadedFile, dbPerformance.DataPath, dbPerformance.PosterName);
            //}

            //if (uploadedProceedings != null)
            //{
            //    dbPerformance.ProceedingsName = Path.GetFileName(uploadedProceedings.FileName);
            //    await UserDataPathWorker.UploadFile(uploadedProceedings, dbPerformance.DataPath, dbPerformance.ProceedingsName);
            //}

            //dbPerformance.KeyWords = performance.KeyWords;
            //dbPerformance.Annotation = performance.Annotation;
            //dbPerformance.Location = performance.Location;
            //dbPerformance.Name = performance.Name;
            //dbPerformance.SectionId = performance.SectionId;
            //dbPerformance.Type = performance.Type;
            //dbPerformance.Start = performance.Start;

            //_db.Entry(dbPerformance).State = EntityState.Modified;
            //await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return RedirectToAction("Error500", "Error");
            }

            if (! await IsPerformanceModifier(performanceId))
            {
                return RedirectToAction("Error500", "Error");
            }

            var path = UserDataPathWorker.GetRootPath(performance.DataPath);

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    return RedirectToAction("Error500", "Error");
                }
                
            }
            
            _db.Performances.Remove(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartDate(DateTime start, int? sectionId)
        {
            if (start < DateTime.Now)
            {
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}");
            }
            if (sectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == sectionId).SingleAsync();
                if (start < section.Start || start > section.End)
                {
                    return Json($"Дата {start} выходит за временные рамки секции {section.Name}.");
                }
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckType(string type)
        {
            if (!DataFactory.GetPerformanceTypes().GetValues().Contains(type))
            {
                return Json($"Тип {type} не существует");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckLocation(string location, bool isSectionStartAndLocation)
        {
            if (!isSectionStartAndLocation && String.IsNullOrEmpty(location))
            {
                return Json($"Выберите адрес");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckStart(DateTime start, bool isSectionStartAndLocation)
        {
            if (!isSectionStartAndLocation && start == null)
            {
                return Json($"Выберите время начала");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckSection(int? sectionId, bool isSectionStartAndLocation)
        {
            if (isSectionStartAndLocation && sectionId == null)
            {
                return Json($"Выберите секцию");
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckStartAndLocation(bool isSectionStartAndLocation, DateTime start, string location, int? sectionId)
        {
            if (!isSectionStartAndLocation)
            {
                if (start == null)
                {
                    return Json($"Выберите время начала");
                }

                if (String.IsNullOrEmpty(location))
                {
                    return Json($"Веберите адрес");
                }
            }
            else
            {
                if (sectionId == null)
                {
                    return Json($"Выберите секцию");
                }

                var section = await _db.Sections.Where(s => s.Id == sectionId).SingleOrDefaultAsync();

                if (section == null)
                {
                    return Json($"Не удалось найти секцию");
                }

                if (section.Start == null)
                {
                    return Json($"Не удалось найти время начала секции");
                }

                if (section.Location == null)
                {
                    return Json($"Не удалось найти адрес секции");
                }
            }

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckSectionExistance(int? sectionId, bool isSectionStartAndLocation)
        {
            if (isSectionStartAndLocation && sectionId == null)
            {
                return Json($"Выберите секцию");
            }

            var section = await _db.Sections.Where(s => s.Id == sectionId).SingleOrDefaultAsync();

            if (section == null)
            {
                return Json($"Не удалось найти секцию");
            }

            if (section.Start == null)
            {
                return Json($"Не удалось найти время начала секции");
            }

            if (section.Location == null)
            {
                return Json($"Не удалось найти адрес секции");
            }

            return Json(true);
        }

        private async Task UnsubscribeUsers(int performanceId)
        {
            var applicationUserPerformances = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId).ToListAsync();

            foreach(var ap in applicationUserPerformances)
            {
                _db.ApplicationUserPerformances.Remove(ap);
            }

            await _db.SaveChangesAsync();
        }

        private async Task<bool> IsPerformanceModifier(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(s => s.Section)
                .SingleOrDefaultAsync();

            var isModifier = false;

            if (performance == null)
            {
                return isModifier;
            }

            var user = await _userManager.GetUserAsync(User);

            if (performance.CreatorId == user.Id)
            {
                isModifier |= true;
            }

            if (performance.Section != null && 
                performance.Section.ManagerId == user.Id)
            {
                isModifier |= true;
            }

            if (User.IsInRole("admin"))
            {
                isModifier |= true;
            }

            return isModifier;
        }
    }
}