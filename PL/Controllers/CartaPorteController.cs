using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    public class CartaPorteController : Controller
    {
        string mode = "PRO";

        [HttpGet]
        public IActionResult Index()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            return View("CartaPorte");
        }

        [HttpPost]
        public IActionResult GetLoadInfo([FromBody] ML.CartaPorte.QueryCarta queryCarta)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.CartaPorte.CartaPorte.GetLoadInfo(queryCarta, mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Object);
        }

        [HttpPost]
        public IActionResult GetColByCodPos(string codPos)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.CartaPorte.CartaPorte.GetColByCodPos(codPos, mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Object);
        }

        [HttpPatch]
        public IActionResult Maintenance([FromBody] ML.CartaPorte.ScnDir scnDir)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.CartaPorte.CartaPorte.Maintenance(scnDir, mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Object);
        }

        [HttpGet]
        public IActionResult GetTrans()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.CartaPorte.CartaPorte.GetTrans(mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Object);
        }

        [HttpGet]
        public IActionResult GetOper()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.CartaPorte.CartaPorte.GetOper(mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Object);
        }

        [HttpPost]
        public IActionResult CartaPorteMaker([FromBody] ML.CartaPorte.CartaInput cartaInput)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                //return RedirectToAction("Login", "Login");
                return BadRequest(new
                {
                    message = $@"Sesion expirada, vuelve a entrar"
                });
            }

            ML.Result result = BL.CartaPorte.CartaPorte.CartaPorteMaker(cartaInput, mode);
            if (!result.Correct)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Message);
        }

    }
}
