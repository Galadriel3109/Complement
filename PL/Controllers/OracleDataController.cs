using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers
{
    public class OracleDataController : Controller
    {
        private readonly IConfiguration _config;

        public OracleDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetCredentials()
        {
            var username = _config["OracleData:Username"];
            var password = _config["OracleData:Password"];

            return Json(new { username, password });
        }


        [HttpGet]
        public IActionResult GetEndPointOLPN()
        {
            string pto_alm = HttpContext.Session.GetString("pto_alm");
            string mode = HttpContext.Session.GetString("mode");

            var endPoint = _config[$@"OracleData:EndPointOLPN{pto_alm}{mode}"];

            return Json(new { endPoint });
        }

        [HttpGet]
        public IActionResult GetEndPointASN()
        {
            string pto_alm = HttpContext.Session.GetString("pto_alm");

            var endPoint = pto_alm == "870" ? _config["OracleData:EndPointASN870"] : _config["OracleData:EndPointASN850"];

            return Json(new { endPoint });
        }


        [HttpGet]
        public IActionResult GetApiKey()
        {
            var apiKey = _config["GoogleData:ApiKey"];

            return Json(new { apiKey });
        }


    }
}
