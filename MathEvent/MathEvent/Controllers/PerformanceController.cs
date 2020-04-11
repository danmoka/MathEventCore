using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index()
        {
            var performances = await _db.Performances
                .Include(p => p.Section)
                .Include(p => p.Creator).ToListAsync();
            //foreach(var performance in performances)
            //{
            //    performance.DataPath = Path.Combine(_webHostEnvironment.WebRootPath, performance.DataPath);
            //    performance.PosterName = Path.Combine(performance.DataPath, performance.PosterName);
            //}
            return View(performances);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            var types = DataFactory.GetPerformanceTypes().GetValues()
                 .Select(x => new SelectListItem { Text = x, Value = x })
                 .ToList();
            ViewBag.Types = types;
            var user = await _userManager.GetUserAsync(User);
            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Name", "Type", "KeyWords", "Annotation", "Start", "SectionId")] Performance performance, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid)
            {
                // создать страницу
            }

            var user = await _userManager.GetUserAsync(User);
            performance.CreatorId = user.Id;
            await _db.Performances.AddAsync(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath;

            if (performance.SectionId != null)
            {
                var section = await _db.Sections.Where(s => s.Id == performance.SectionId).FirstOrDefaultAsync();
                performanceDataPath = section.DataPath;
            }
            else
            {
                performanceDataPath = user.DataPath;
            }

            UserDataPathWorker.CreateSubDirectory(ref performanceDataPath, performance.Id.ToString());
            performance.DataPath = performanceDataPath;

            if (uploadedFile != null)
            {
                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await uploadedFile.CopyToAsync(fileStream);
                //fileUpload.SaveAs(Path.Combine(performance.DataPath, performance.PosterName));
            }
            else
            {
                performance.PosterName = Path.GetFileName(UserDataPathWorker.GetDefaultImagePath());
                using var deafaultImg = new FileStream(UserDataPathWorker.GetRootPath(UserDataPathWorker.GetDefaultImagePath()), FileMode.Open);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await deafaultImg.CopyToAsync(fileStream);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Card(int performanceId)
        {
            // если perfId будет null, то что?

            ViewBag.SignedUp = "Записаться";

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

                if (ap != null)
                {
                    ViewBag.SignedUp = "Отписаться";
                }
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceId)
                .Include(p => p.Section)
                .Include(p => p.Creator).FirstAsync();

            return View(performance);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Subscribe(int performanceId)
        {
            // если perfId == null?

            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

            if (ap == null)
            {
                ap = new ApplicationUserPerformance()
                {
                    ApplicationUserId = user.Id,
                    PerformanceId = (int) performanceId
                };

                await _db.ApplicationUserPerformances.AddAsync(ap);
                var performance = await _db.Performances.Where(p => p.Id == performanceId).FirstAsync(); // или лучше триггер?
                performance.Traffic++;
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }
            else
            {
                _db.ApplicationUserPerformances.Remove(ap);
                var performance = await _db.Performances.Where(p => p.Id == performanceId).FirstAsync(); // или лучше триггер?
                performance.Traffic--; // если меньше 0 получается, то это ошибка проектирования (наверное) где-то, в бд стоит check(>= 0). Но будет он отрабатывать - неизвестно. Но это не его проблемы, решать эту проблему надо тут.
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Card", "Performance", new { performanceId = performanceId});
        }

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> UnSubscribe(int? performanceId)
        //{
        //    if (performanceId == null)
        //    {
        //        // сделать HttpNotFound
        //        return RedirectToAction("Index", "Home");
        //    }

        //    var user = await _userManager.GetUserAsync(User);
        //    var userId = user.Id;

        //    var ap = await _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == performanceId && ap.ApplicationUserId == userId).FirstOrDefaultAsync();

        //    if (ap != null)
        //    {
        //        _db.ApplicationUserPerformances.Remove(ap);
        //        var performance = await _db.Performances.Where(p => p.Id == performanceId).FirstAsync(); // или лучше триггер?
        //        performance.Traffic--;
        //        _db.Performances.Update(performance);
        //        await _db.SaveChangesAsync();
        //    }
        //    // если еще не записан, то вывести что-нибудь

        //    return RedirectToAction("Card", "Performance", new { performanceId = performanceId });
        //}

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int performanceId)
        {
            // если id null, то что?
            var performance = await _db.Performances.Where(c => c.Id == performanceId).FirstAsync();
            // если не нашли, то что?

            var user = await _userManager.GetUserAsync(User);
            var userSections = await _db.Sections.Where(s => s.ManagerId == user.Id).ToListAsync();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View(performance);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id", "Name", "Type", "KeyWords", "Annotation", "Start", "SectionId",
            "CreatorId", "DataPath", "PosterName", "Traffic")] Performance performance, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid)
            {
                // создать страницу
            }

            if (uploadedFile != null)
            {
                var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName));
                if (System.IO.File.Exists(imageToBeDeleted))
                {
                    System.IO.File.Delete(imageToBeDeleted);
                }

                performance.PosterName = Path.GetFileName(uploadedFile.FileName);
                using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
                await uploadedFile.CopyToAsync(fileStream);
                //fileUpload.SaveAs(Path.Combine(performance.DataPath, performance.PosterName));
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int performanceId)
        {
            // если perfId == null?

            var performance = await _db.Performances.Where(p => p.Id == performanceId).FirstAsync();

            if (System.IO.File.Exists(performance.DataPath))
            {
                System.IO.File.Delete(performance.DataPath);
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
                return Json($"Дата {start} меньше текущей даты {DateTime.Now}.");
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

        //[HttpPost]
        //[ActionName("ChangePoster")]
        //public async Task<IActionResult> ChangePoster(int performanceId, IFormFile uploadedFile)
        //{
        //    var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

        //    if (uploadedFile != null)
        //    {
        //        var imageToBeDeleted = UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName));
        //        if (System.IO.File.Exists(imageToBeDeleted))
        //        {
        //            System.IO.File.Delete(imageToBeDeleted);
        //        }

        //        performance.PosterName = Path.GetFileName(uploadedFile.FileName);
        //        using var fileStream = new FileStream(UserDataPathWorker.GetRootPath(Path.Combine(performance.DataPath, performance.PosterName)), FileMode.Create);
        //        await uploadedFile.CopyToAsync(fileStream);
        //        //fileUpload.SaveAs(Path.Combine(performance.DataPath, performance.PosterName));
        //        _db.Performances.Update(performance);
        //        await _db.SaveChangesAsync();
        //    }

        //    return RedirectToAction("Edit", "Performance", new { performanceId = performanceId });
        //}
    }
}