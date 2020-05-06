using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathEvent.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MathEvent.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProceedingsController : ControllerBase
    {
        private readonly ApplicationContext _db;

        public ProceedingsController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("deleteProceedings")]
        public async Task DeleteProceedings(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return; ;
            }

            var file = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(performance.DataPath, performance.ProceedingsName));

            if (System.IO.File.Exists(file))
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch
                {
                    return;
                }

                performance.ProceedingsName = null;
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return;
        }

        [HttpGet]
        [Route("downloadProceedings")]
        public async Task<IActionResult> DownloadProceedings(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance != null)
            {
                var filePath = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(
                    performance.DataPath, performance.ProceedingsName));
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, "application/pdf", performance.ProceedingsName);
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("isFileExists")]
        public async Task<bool> IsFileExists(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (string.IsNullOrEmpty(performance.ProceedingsName))
            {
                return false;
            }

            return true;
        }
    }
}