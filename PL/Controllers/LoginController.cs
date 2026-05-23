using Microsoft.AspNetCore.Mvc;
using System.Data.Odbc;

namespace PL.Controllers
{
    public class LoginController : Controller
    {
        string mode = "PRO";
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View(new ML.Login.Login());
        }
        [HttpPost]
        public ActionResult Login(ML.Login.Login login)
        {
            ML.Result result = BL.Login.Login.Log(login,mode);
            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
                return View(login);
            }
            ML.Login.Login loged = (ML.Login.Login)result.Object;

            HttpContext.Session.SetString("usu_id", loged.usu_id ?? "");
            HttpContext.Session.SetString("usu_nombre", loged.usu_nombre ?? "");
            HttpContext.Session.SetString("cv_area", loged.cv_area ?? "");
            HttpContext.Session.SetString("nombre", loged.nombre ?? "");
            HttpContext.Session.SetString("sub_rol", loged.sub_rol ?? "");
            HttpContext.Session.SetString("pto_alm", loged.pto_alm ?? "");
            HttpContext.Session.SetString("almacenes", loged.almac);
            HttpContext.Session.SetString("mode", mode);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public IActionResult ChangeAlmacen([FromBody] string pto_alm)
        {
            var usuId = HttpContext.Session.GetString("usu_id");

            if (string.IsNullOrEmpty(usuId))
                return Unauthorized();

            HttpContext.Session.SetString("pto_alm", pto_alm);

            return Json(new { success = true });
        }
    }
}
