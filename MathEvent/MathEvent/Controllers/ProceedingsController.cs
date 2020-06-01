using System.Linq;
using System.Threading.Tasks;
using MathEvent.Models;
using Microsoft.AspNetCore.Mvc;
using MathEvent.Helpers;
using Microsoft.EntityFrameworkCore;
using MathEvent.Models.ViewModels;
using System.Net;
using MathEvent.Helpers.Access;

namespace MathEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProceedingsController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserService _userService;

        public ProceedingsController(ApplicationContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        [HttpPost]
        [Route("deleteProceedings")]
        public async Task<HttpStatusCode> DeleteProceedings(
            [Bind("Id")] PerformanceViewModel performanceModel)
        {
            if (!ModelState.IsValid)
            {
                return HttpStatusCode.BadRequest;
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceModel.Id).SingleOrDefaultAsync();

            if (performance == null)
            {
                return HttpStatusCode.NotFound;
            }

            var filePath = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(performance.DataPath, performance.ProceedingsName));

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                    return HttpStatusCode.NotFound;
                }

                performance.ProceedingsName = null;
            }

            _db.Performances.Update(performance);
            await _db.SaveChangesAsync();

            return HttpStatusCode.OK;
        }

        [HttpGet]
        [Route("downloadProceedings")]
        public async Task<IActionResult> DownloadProceedings(int performanceId)
        {
            var performance = await _db.Performances.Where(p => p.Id == performanceId).SingleOrDefaultAsync();

            if (performance == null)
            {
                return NotFound();
            }

            var filePath = UserDataPathWorker.GetRootPath(UserDataPathWorker.ConcatPaths(
                    performance.DataPath, performance.ProceedingsName));
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, "application/pdf", performance.ProceedingsName);
        }

        [HttpPost]
        [Route("isFileExists")]
        public async Task<bool> IsFileExists(
            [Bind("Id", "UserId", "UserRoles")] PerformanceViewModel performanceViewModel)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            var performance = await _db.Performances.Where(p => p.Id == performanceViewModel.Id)
                .Include(s => s.Section)
                .SingleOrDefaultAsync();

            var isExist = false;

            if (performance == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(performance.ProceedingsName))
            {
                isExist = true;
            }

            var isModifier = await _userService.IsPerformanceModifier(performance.Id, performanceViewModel.UserId);

            return isModifier && isExist;
        }
    }
}