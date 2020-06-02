using System.Linq;
using MathEvent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace MathEvent.Controllers
{
    /// <summary>
    /// Контроллер для скачивания программы конференции в формате pdf
    /// Работает только на localhost
    /// </summary>
    public class PdfController : Controller
    {
        private readonly ApplicationContext _db;
        public PdfController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult TimeTable(int conferenceId)
        {
            var conference = _db.Conferences.Where(c => c.Id == conferenceId)
                .Include(c => c.Sections)
                .ThenInclude(s => s.Performances)
                .SingleOrDefault();

            if (conference == null)
            {
                return RedirectToAction("Error404", "Error");
            }


            //return new ViewAsPdf(UserDataPathWorker.ConcatPaths(UserDataPathWorker.GetPdfTemplatesDirectory(), "ConferenceTimeTable.cshtml"), conference);
            return View(conference);
        }
    }
}