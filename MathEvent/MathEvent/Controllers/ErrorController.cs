using Microsoft.AspNetCore.Mvc;

namespace MathEvent.Controllers
{
    //todo: сделать красивые View
    public class ErrorController : Controller
    {
        [Route("error/404")]
        public IActionResult Error404()
        {
            return Content("404");
        }

        [Route("error/400")]
        public IActionResult Error400()
        {
            return Content("400");
        }

        [Route("error/500")]
        public IActionResult Error500()
        {
            return Content("500");
        }

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            // handle different codes or just return the default error view
            return Content($"default: {code}");
        }
    }
}