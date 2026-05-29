using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult Prohibido()
        {
            Response.StatusCode = 403;
            return View();
        }

        [HttpGet]
        public ActionResult NoEncontrado()
        {
            Response.StatusCode = 404;
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
