using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
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
        private readonly ApplicationContext _db;
        private static IWebHostEnvironment _webHostEnvironment;

        public PerformanceController(ApplicationContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
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
        public async Task<IActionResult> Add()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var types = DataFactory.GetPerformanceTypes().GetValues()
                 .Select(x => new SelectListItem { Text = x, Value = x })
                 .ToList();
            ViewBag.Types = types;
            var user = await _userManager.GetUserAsync(User);
            var userSections = _db.Sections.Where(s => s.ManagerId == user.Id).ToList();
            ViewBag.Sections = new SelectList(userSections, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Performance performance, IFormFile uploadedFile)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            performance.CreatorId = user.Id;
            _db.Performances.Add(performance);
            await _db.SaveChangesAsync();

            string performanceDataPath;

            if (performance.SectionId != null)
            {
                var section = _db.Sections.Where(s => s.Id == performance.SectionId).FirstOrDefault();
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
    }
}