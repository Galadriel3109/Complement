using ML.BaseControl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BL.AsnHelper
{
    public class AsnHelper
    {
        public static ML.Result GetAsnInfoByLoad(ML.AsnHelper.Load load, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                ML.Result resultGetOracleCode = GetOracleCode(load.CodPto, mode);
                if (!resultGetOracleCode.Correct)
                {
                    throw new Exception($@"{resultGetOracleCode.Message}");
                }
                load.Facility = (string)resultGetOracleCode.Object;

                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($@"{DL.ApiOracle.GetOracleUsr(mode)}:{DL.ApiOracle.GetOraclePwd(mode)}");
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    string url = $"{DL.ApiOracle.GetASNPerLoad(mode).Replace("{$$$}", load.Facility).Replace("{#####}", load.LoadNbr)}";

                    var response = client.GetAsync(url).Result;
                    var json = response.Content.ReadAsStringAsync().Result;

                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<ML.AsnHelper.AsnResponse>(json);

                    if (data != null && data.result_count > 0)
                    {
                        List<ML.AsnHelper.AsnInfo> lista = data.results
                        .Select(x => new ML.AsnHelper.AsnInfo
                        {
                            Id = x.Id,
                            NumAsn = x.NumAsn,
                            Factura = x.Factura,
                            Monto = x.Monto.ToString(),
                            Pais = x.Pais,
                            NAR = x.NAR
                        })
                        .ToList();

                        result.Object = lista;
                    }
                    else
                    {
                        result.Object = new List<ML.AsnHelper.AsnInfo>();
                    }
                }

                result.Correct = true;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar carga {load.LoadNbr}, favor de validar {ex.Message}";
            }
            return result;
        }
        private static ML.Result GetOracleCode(string cod_pto, string mode)
        {
            ML.Result result = new ML.Result();
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string query = $@"SELECT CASE WHEN (B.facility IS NULL)
                                            THEN
                                                    'SRS'||A.cod_pto
                                            ELSE
                                                    TRIM(B.facility)
                                            END
                                    FROM puntos A,OUTER ora_fac_go B
                                    WHERE A.cod_emp = 1
                                    AND A.cod_pto = {cod_pto}
                                    AND B.cen_pto = A.cod_pto
                                    AND B.is_fac = 'T'";

                    using (OdbcCommand cmd = new OdbcCommand(query, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string originCode = reader.GetString(0);
                                result.Object = originCode;
                            }
                            else
                            {
                                throw new Exception("No se leyó el origin code");
                            }
                        }
                    }
                    result.Correct = true;
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = "Error al obtener el origin code " + ex.Message;
                result.Ex = ex;
            }
            return result;
        }

        public static ML.Result GetLegacyInfo(ML.AsnHelper.AsnInfo asnInfo, string mode)
        {
            ML.Result result = new ML.Result();
            mode = "PRO";
            try
            {
                using (OdbcConnection connection = new OdbcConnection(DL.Connection.GetConnectionStringGen(mode)))
                {
                    connection.Open();

                    string query = $@"SELECT fol_acuse_rec NAR,TRIM(serie)||TRIM(folio) AS factura, total AS monto
                                        FROM fac_elec
                                        WHERE idcompania = 24
                                        AND marca = '0'
                                        AND fol_acuse_rec = '{asnInfo.NAR.Trim()}'
                                        AND proveedorid IN (SELECT MOD(cod_pro,1000000)
                                                            FROM pedcab
                                                            WHERE cod_emp = 1
                                                            ANd pto_emi = 999
                                                            AND num_ped = {asnInfo.NumAsn.Split('_')[0]}
                                                            )
                                        ";

                    using (OdbcCommand cmd = new OdbcCommand(query, connection))
                    {
                        using (OdbcDataReader reader = cmd.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                asnInfo.NAR = reader.GetString(0).Trim();
                                asnInfo.Factura = reader.GetString(1);
                                asnInfo.Monto = reader.GetString(2);
                                asnInfo.Pais = "MEX";
                            }
                            else
                            {
                                throw new Exception($@"No se localizo el NAR {asnInfo.NAR} favor de validar que se apto y compatible");
                            }
                        }
                    }
                    result.Correct = true;
                    result.Object = asnInfo;
                }

                result.Correct = true;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al recuperar el NAR {asnInfo.NAR}, favor de validar {ex.Message}";
            }
            return result;
        }

        public static async Task<ML.Result> PatchAsnById(ML.AsnHelper.AsnInfo asnInfo, string mode)
        {
            ML.Result result = new ML.Result();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] byteArray = Encoding.ASCII.GetBytes($"{DL.ApiOracle.GetOracleUsr(mode)}:{DL.ApiOracle.GetOraclePwd(mode)}");

                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    string url = $"{DL.ApiOracle.PatchAsnById(mode).Replace("{id}", asnInfo.Id)}";

                    var body = new
                    {
                        fields = new
                        {
                            cust_field_1 = asnInfo.Factura,
                            cust_field_2 = asnInfo.Monto.ToString(),
                            cust_field_3 = asnInfo.Pais,
                            cust_field_4 = asnInfo.NAR
                        }
                    };

                    var json = JsonConvert.SerializeObject(body);

                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };

                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        result.Correct = false;
                        string messageResponse = await response.Content.ReadAsStringAsync();
                        throw new Exception(messageResponse);
                    }
                }

                result.Correct = true;
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = $@"Error al actualizar campos en WMS para ASN {asnInfo.NumAsn}: {ex.Message}";
            }

            return result;
        }
    }
}
