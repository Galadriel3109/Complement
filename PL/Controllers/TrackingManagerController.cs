using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ML.Maintenance;
using System;
using System.Net.Http.Headers;
using System.Text;
using static NuGet.Packaging.PackagingConstants;

namespace PL.Controllers
{
    public class TrackingManagerController : Controller
    {
        string mode = "PRO";

        private readonly IConfiguration _config;

        public TrackingManagerController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetTrackingPerDay()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            string ptoAlm = HttpContext.Session.GetString("pto_alm");
            DateTime today = DateTime.Today;

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            List<ML.TrackingManager.OutboundShipment> routeList = new List<ML.TrackingManager.OutboundShipment>();
            ML.Result result = BL.TrackingManager.TrackingManager.GetTrackingPerDay(today.ToString("ddMMyyyy"), cod_pto, mode);

            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
                return View("Rutas", new List<ML.TrackingManager.OutboundShipment>());
            }

            routeList = (List<ML.TrackingManager.OutboundShipment>)result.Object;
            return View("Rutas", routeList);
        }

        [HttpPost]
        public IActionResult GetTrackingPerDay([FromBody] ML.TrackingManager.DateRequest request)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }
            List<ML.TrackingManager.OutboundShipment> routeList = new List<ML.TrackingManager.OutboundShipment>();

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            ML.Result result = BL.TrackingManager.TrackingManager.GetTrackingPerDay(request.date, cod_pto, mode);
            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
            }
            routeList = (List<ML.TrackingManager.OutboundShipment>)result.Object;

            //return View("Rutas", routeList);
            return Json(routeList);
        }

        [HttpPost]
        public IActionResult GetOrdersPerOutboundShipment(ML.TrackingManager.OutboundShipment outboundShipment)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }
            List<ML.TrackingManager.TrackingManager> trackingManagertList = new List<ML.TrackingManager.TrackingManager>();

            ML.Result result = BL.TrackingManager.TrackingManager.GetOrdersPerOutboundShipment(outboundShipment, mode);
            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
            }
            trackingManagertList = (List<ML.TrackingManager.TrackingManager>)result.Object;

            ViewBag.FechaSeleccionada = outboundShipment.fechaSeleccionada;

            return View("Ruta", trackingManagertList);
        }

        [HttpPost]
        public IActionResult GetDetail(string ord_rel)
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }

            ML.Result result = BL.TrackingManager.TrackingManager.GetDetail(ord_rel, mode);
            if (!result.Correct)
            {
                //ViewBag.Error = result.Message;
                return PartialView("_DetalleError", result.Message);
            }
            var olpnList = (List<string>)result.Object;

            return PartialView("_DetalleModal", olpnList);
        }

        //GetReturnedOrders
        [HttpGet]
        public IActionResult ReturnedOrders()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }
            List<ML.TrackingManager.TrackingManager> trackingManagertList = new List<ML.TrackingManager.TrackingManager>();

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            ML.Result result = BL.TrackingManager.TrackingManager.GetReturnedOrders(cod_pto, mode);
            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
            }
            trackingManagertList = (List<ML.TrackingManager.TrackingManager>)result.Object;

            return View("ReturnedOrders", trackingManagertList);
        }

        [HttpGet]
        public IActionResult ReturnedOrdersExcel()
        {
            var usuId = HttpContext.Session.GetString("usu_id");
            if (string.IsNullOrEmpty(usuId))
            {
                return RedirectToAction("Login", "Login");
            }
            List<ML.TrackingManager.TrackingManager> trackingManagertList = new List<ML.TrackingManager.TrackingManager>();

            string cod_pto = HttpContext.Session.GetString("pto_alm");

            ML.Result result = BL.TrackingManager.TrackingManager.GetReturnedOrders(cod_pto, mode);
            if (!result.Correct)
            {
                ViewBag.Error = result.Message;
            }
            trackingManagertList = (List<ML.TrackingManager.TrackingManager>)result.Object;

            ViewBag.Error = result.Message;

            trackingManagertList = (List<ML.TrackingManager.TrackingManager>)result.Object;

            trackingManagertList = GetStatusWms(trackingManagertList, cod_pto);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Embarques");

                ws.Cell(1, 1).Value = "Fecha Carga";
                ws.Cell(1, 2).Value = "Fecha Actualizacion";
                ws.Cell(1, 3).Value = "Carga de salida";
                ws.Cell(1, 4).Value = "Orden Release";
                ws.Cell(1, 5).Value = "Sales Check";
                ws.Cell(1, 6).Value = "Cliente";
                ws.Cell(1, 7).Value = "Estatus";
                ws.Cell(1, 8).Value = "Estatus_ord_rel";
                ws.Cell(1, 9).Value = "Estado";
                ws.Cell(1, 10).Value = "Estado_gnx";
                ws.Cell(1, 11).Value = "Rt_stat";
                ws.Cell(1, 12).Value = "Estado_rt";
                ws.Cell(1, 13).Value = "Cod_mot";
                ws.Cell(1, 14).Value = "Motivo";
                ws.Cell(1, 15).Value = "EstadoWms";
                ws.Cell(1, 16).Value = "Fecha Verificacion";

                int row = 2;
                if (trackingManagertList.Count > 0)
                {
                    foreach (ML.TrackingManager.TrackingManager trManager in trackingManagertList)
                    {
                        ws.Cell(row, 1).Value = trManager.fec_car ?? "-";
                        ws.Cell(row, 2).Value = trManager.fec_act ?? "-";
                        ws.Cell(row, 3).Value = trManager.car_sal ?? "-";
                        ws.Cell(row, 4).Value = trManager.ord_rel ?? "-";
                        ws.Cell(row, 5).Value = trManager.num_scn ?? "-";
                        ws.Cell(row, 6).Value = trManager.cliente ?? "-";
                        ws.Cell(row, 7).Value = trManager.estatus.ToString() ?? "-";
                        ws.Cell(row, 8).Value = trManager.estatus_ord_rel ?? "-";
                        ws.Cell(row, 9).Value = trManager.estado ?? "-";
                        ws.Cell(row, 10).Value = trManager.estado_gnx ?? "-";
                        ws.Cell(row, 11).Value = trManager.rt_stat ?? "-";
                        ws.Cell(row, 12).Value = trManager.estado_rt ?? "-";
                        ws.Cell(row, 13).Value = trManager.cod_mot ?? "-";
                        ws.Cell(row, 14).Value = trManager.motivo ?? "-";
                        ws.Cell(row, 15).Value = trManager.estadoWms ?? "-";
                        ws.Cell(row, 16).Value = trManager.fecVerifica ?? "-";

                        row++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"ASN_Regresos_Sin_Verificar_{usuId}_{DateTime.Today.ToString("yyyyMMdd")}.xlsx");
                }
            }


        }

        public List<ML.TrackingManager.TrackingManager> GetStatusWms(List<ML.TrackingManager.TrackingManager> trackingManagertList, string cod_pto)
        {
            using (var client = new HttpClient())
            {
                var username = _config["OracleData:Username"];
                var password = _config["OracleData:Password"];

                string endPointBru = cod_pto == "870" ? _config["OracleData:EndPointASN870"] : _config["OracleData:EndPointASN850"];


                var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                foreach(var tracking in  trackingManagertList)
                {
                    try
                    {
                        string url = endPointBru.Replace("ORDER_RELEASE", tracking.ord_rel);

                        var response = client.GetAsync(url).Result;
                        var json = response.Content.ReadAsStringAsync().Result;

                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                        if (data.result_count > 0)
                        {
                            tracking.estadoWms = data.results[0].Estatus == 50 ? "Verificado" : "No Verificado";
                            tracking.fecVerifica = data.results[0].fecVE;
                        }
                    }
                    catch
                    {
                        tracking.estadoWms = "No se logro consultar";
                    }

                }
            }
            
            return trackingManagertList;
        }
    }
}
