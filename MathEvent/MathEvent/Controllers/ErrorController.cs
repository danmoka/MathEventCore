using Microsoft.AspNetCore.Mvc;

namespace MathEvent.Controllers
{
    /// <summary>
    /// Контроллер ошибок
    /// </summary>
    public class ErrorController : Controller
    {
        [Route("error/404")]
        public IActionResult Error404()
        {
            ViewBag.ImageName = "error404.png";
            ViewBag.Message = "";
            return View();
        }

        [Route("error/403")]
        public IActionResult Error403()
        {
            ViewBag.ImageName = "error403.png";
            ViewBag.Message = "";
            return View();
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            ViewBag.ImageName = "error500.png";
            ViewBag.Message = "";
            return View();
        }

        /// <summary>
        /// Обработка остальных ошибок
        /// </summary>
        /// <param name="code">Код ошибки</param>
        /// <returns>Представление ошибки</returns>
        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            ViewBag.Message = $"Ошибка {code}";
            return View();
        }
    }
}