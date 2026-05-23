using BL.Menu;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    public class UserController : Controller
    {
        string mode = "PRO";

        [HttpGet]
        public IActionResult User()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            return View("User");
        }
        

        [HttpGet]
        public IActionResult Rol()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            return View("Rol");
        }
        
        [HttpGet]
        public IActionResult GetMenu()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            //if (string.IsNullOrEmpty(usuId))
            //{
            //    return RedirectToAction("Login", "Login");
            //}

            string cv_area = HttpContext.Session.GetString("cv_area");
            string sub_rol = HttpContext.Session.GetString("sub_rol");

            if (cv_area != null)
            {
                List<MenuItem> menuList = BL.User.User.GetMenu(usuId, sub_rol, mode);
                return Json(menuList);
            }
            else
            {
                return Json($@"");
            }
        }
        
        [HttpGet]
        public IActionResult GetLgaUsu(string usu_id)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.GetLgaUsu(usu_id, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json(result.Object);
        }
        
        [HttpGet]
        public IActionResult GetAlma()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.GetAlma(mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json(result.Object);
        }
        
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.GetAllUsers(mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json(result.Object);
        }
        
        [HttpPost]
        public IActionResult AddUser([FromBody]ML.User.User user)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.AddUser(user, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }
        
        [HttpGet]
        public IActionResult GetRoles()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            string sub_rol = HttpContext.Session.GetString("sub_rol");

            ML.Result result = BL.User.User.GetRoles(sub_rol, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json(result.Object);
        }


        [HttpPatch]
        public IActionResult UpdateUserRol([FromBody] ML.User.User user)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.UpdateUserRol(user, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }

        [HttpPost]
        public IActionResult AddRol([FromBody] ML.User.Rol rol)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.AddRol(rol, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }

        [HttpDelete]
        public IActionResult DelRol([FromBody] ML.User.Rol rol)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.DelRol(rol, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }

        [HttpGet]
        public IActionResult GetScreens(string sub_rol)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.GetScreens(sub_rol, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json(result.Object);
        }

        [HttpPost]
        public IActionResult AddScreenToRol(string sub_rol, string id_pant)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.AddScreenToRol(sub_rol, id_pant, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }

        [HttpDelete]
        public IActionResult DelScreenToRol(string sub_rol, string id_pant)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.User.User.DelScreenToRol(sub_rol, id_pant, mode);
            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }
            return Json($@"Exitoso");
        }

    }
}
