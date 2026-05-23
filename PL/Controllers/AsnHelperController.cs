using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    public class AsnHelperController : Controller
    {
        string mode = "PRO";

        [HttpGet]
        public IActionResult ValidaFacturaNar()
        {
            string usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetAsnInfoByLoad(string loadNbr)
        {
            string usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            ML.AsnHelper.Load load = new ML.AsnHelper.Load
            {
                LoadNbr = loadNbr,
                CodPto = cod_pto,
            };

            ML.Result result = BL.AsnHelper.AsnHelper.GetAsnInfoByLoad(load, mode);

            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Object);
        }

        [HttpPost]
        public IActionResult GetLegacyInfo([FromBody] ML.AsnHelper.AsnInfo asnInfo)
        {
            string usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.AsnHelper.AsnHelper.GetLegacyInfo(asnInfo, mode);

            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Object);
        }

        [HttpPatch]
        public async Task<IActionResult> PatchAsnById([FromBody] ML.AsnHelper.AsnInfo asnInfo)
        {
            string usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return Unauthorized();
            }


            ML.Result result = await BL.AsnHelper.AsnHelper.PatchAsnById(asnInfo, mode);

            if (!result.Correct)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}
