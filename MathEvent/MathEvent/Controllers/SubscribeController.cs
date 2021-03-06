﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    /// <summary>
    /// API контроллер для записи/отписка на событие
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribeController : Controller
    {
        private readonly ApplicationContext _db;

        public SubscribeController(ApplicationContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Записывает/отписывает пользователя на/от события
        /// </summary>
        /// <param name="model">Вью-модель события</param>
        /// <returns>Статус код результата обработки</returns>
        [HttpPost]
        [Route("signup")]
        public async Task<HttpStatusCode> SignUp(
            [Bind("Id", "UserId")]PerformanceViewModel model)
        {
            var ap = await _db.ApplicationUserPerformances
                .Where(ap => ap.PerformanceId == model.Id && ap.ApplicationUserId == model.UserId)
                .SingleOrDefaultAsync();

            /// еще не подписан
            if (ap == null)
            {
                ap = new ApplicationUserPerformance()
                {
                    ApplicationUserId = model.UserId,
                    PerformanceId = model.Id
                };

                _db.ApplicationUserPerformances.Add(ap);
                var performance = await _db.Performances
                    .Where(p => p.Id == model.Id).
                    SingleOrDefaultAsync(); // или лучше триггер?

                if (performance == null)
                {
                    return HttpStatusCode.NotFound;
                }

                performance.Traffic++;
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }
            else /// если подписан
            {
                _db.ApplicationUserPerformances.Remove(ap);
                var performance = await _db.Performances
                    .Where(p => p.Id == model.Id)
                    .SingleOrDefaultAsync(); // или лучше триггер?

                if (performance == null)
                {
                    return HttpStatusCode.NotFound;
                }

                performance.Traffic--; // если меньше 0 получается, то это ошибка проектирования (наверное) где-то, в бд стоит check(>= 0). Но будет он отрабатывать - неизвестно. Но это не его проблемы, решать эту проблему надо тут.
                // в модели стоит Range, должен отработать скрипт, что значение меньше 0
                _db.Performances.Update(performance);
                await _db.SaveChangesAsync();
            }

            return HttpStatusCode.OK;
        }
    }
}