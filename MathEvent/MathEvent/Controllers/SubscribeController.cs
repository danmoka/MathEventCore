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

namespace MathEvent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationContext _db;

        public SubscribeController(ApplicationContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUp(CardViewModel model)
        {
            var ap = _db.ApplicationUserPerformances.Where(ap => ap.PerformanceId == model.Id && ap.ApplicationUserId == model.UserId).SingleOrDefault();

            if (ap == null)
            {
                ap = new ApplicationUserPerformance()
                {
                    ApplicationUserId = model.UserId,
                    PerformanceId = model.Id
                };

                _db.ApplicationUserPerformances.Add(ap);
                var performance = _db.Performances.Where(p => p.Id == model.Id).Single(); // или лучше триггер?
                performance.Traffic++;
                _db.Performances.Update(performance);
                _db.SaveChanges();
            }
            else
            {
                _db.ApplicationUserPerformances.Remove(ap);
                var performance = _db.Performances.Where(p => p.Id == model.Id).Single(); // или лучше триггер?
                performance.Traffic--; // если меньше 0 получается, то это ошибка проектирования (наверное) где-то, в бд стоит check(>= 0). Но будет он отрабатывать - неизвестно. Но это не его проблемы, решать эту проблему надо тут.
                // в модели стоит Range, должен отработать скрипт, что значение меньше 0
                _db.Performances.Update(performance);
                _db.SaveChanges();
            }

            return Ok();
        }
    }
}